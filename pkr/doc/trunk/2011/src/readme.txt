This directory is the root for helper projects that create artefacts (pictures, etc.) for articles in
the parent document. Usually this is done by creating a NUnit unit test and a couple of 
helper classes that can be run either from Visual Studion or with maven. 
It may be necessary to post-process the output of these unit tests. We strive that all projects are 
buildable with a single command:

mvn compile

Some folders can deviate from this rule, for instance, some contains hand-made or copy-pasted files and do not have
be built.

The document is one level above in the directory tree and add the artifacts as links.

