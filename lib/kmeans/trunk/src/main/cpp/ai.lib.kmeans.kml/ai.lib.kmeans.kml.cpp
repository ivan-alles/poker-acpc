// ai.lib.kmeans.kml.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ai.lib.kmeans.kml.h"
#include "KMlocal.h"			// k-means algorithms

extern "C"
{

#pragma pack (push)
#pragma pack (1) 
struct Parameters
{
	//
	// Input
	//

	// number of centers
	int	k;      

	// dimension
	int	dim;

	// Point coordinates for all dimensions. For example, for N points and dim = 2:
	// (p0,0 p0,1) (p1,0 p1,1) ... (pN-1,0 pN-1,1)
	double * points; 

	// Number of points
	int	n; 


	// Max stages to run (e.g. 100 0 0 0).
	double term_st_a, term_st_b, term_st_c, term_st_d;	
	
	double term_minConsecRDL;
	
	double  term_minAccumRDL;
	
	int term_maxRunStage;

	double term_initProbAccept; 

	int term_tempRunLength;

	double term_tempReducFact;

	/// Rng seed, must be a positive number.
	int seed;

	//
	// Output
	//

    // Centers coordinates for all dimensions. Layout as in Parameters.
	double * centers; 
};
#pragma pack (pop)

/// Runs a Hybrid kml algorithm.
AILIBKMEANSKML_API int KML_Hybrid(Parameters * params)
{
	kmIdum = params->seed;
	// Make negate to initialize.
	if(kmIdum > 0) kmIdum = -kmIdum;
	if(kmIdum == 0) kmIdum = -1;


	//  Termination conditions
	KMterm	term(params->term_st_a, 
		params->term_st_b, 
		params->term_st_c, 
		params->term_st_d,
		params->term_minConsecRDL,	
		params->term_minAccumRDL,	
		params->term_maxRunStage,	
		params->term_initProbAccept,
		params->term_tempRunLength,	
		params->term_tempReducFact);

	//term.setAbsMaxTotStage(params->stages);		// set number of stages

	KMdata dataPts(params->dim, params->n);	// allocate data storage

	int i = 0;
	for(int p = 0; p < params->n; ++p)
	{
		for(int d = 0; d < params->dim; ++d)
		{
			dataPts[p][d] = params->points[i++];
		}
	}

    dataPts.setNPts(params->n);		// set actual number of pts
    dataPts.buildKcTree();			// build filtering structure

    KMfilterCenters ctrs(params->k, dataPts); // allocate centers

    KMlocalHybrid kmHybrid(ctrs, term);       // Hybrid heuristic
    ctrs = kmHybrid.execute();

	i = 0;
	for(int p = 0; p < params->k; ++p)
	{
		for(int d = 0; d < params->dim; ++d)
		{
			params->centers[i++] = ctrs[p][d];
		}
	}

	return 1;
}

}