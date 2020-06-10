A package to build hand strength LUTs. This takes about 20 hours 
and the tables are about 140 M in size, therefore they are in a separate package.
This package is in the same maven group. I placed it inside hs package because
they are highly dependent on each other.

To resolve cyclic dependency at the very first build of the main package, call
lut-create-dummy.bat.

To avoid destroying good LUTs, generation is skipped by default. 
To generate new LUTs, call:
 
mvn compile -Dskip-gen=false

Remarks:
This solution with 2 packages was invented when HS was a small part of a big and developing package. It was 
often necessary to make clean builds and snapshots of this package. Since HS was placed in its own package, 
I would prefer another solution with only one package: 
1. Compile sources
2. Test functions that do not use LUT (including those that are used in LUT generation).
3. Generate LUT (if not exists yet) - may take 1 day.
4. Test functions that use LUT
5. As soon it is stable, make a release.

The old solution is kept due to historical reasons. Maybe the whole package will be obsoleted or loses its 
importance soon, therefore I do not want to invest much time in refinements here.

