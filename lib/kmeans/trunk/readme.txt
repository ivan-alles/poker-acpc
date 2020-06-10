K-means clustering. Source: http://www.cs.umd.edu/~mount/Projects/KMeans/

I made the following changes:

- Restructured the sources, so that the most of them are in the kmllib
- Moved VS projects to src/main/cpp/proj-name. Now only kmltest is built,
  it is used for verification and for interactive calculations.
- Moved tests to src\test\test-data-orig, test-data contains the copy with updated 
  results for windows (differ from the original because of the different RNG).
- The other things are to provide a C# interface to kml.

 


