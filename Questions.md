# Questions

Here is a list of the type of questions, that gitbot can (we hope by the end of may?) answer.  
The LUIS Model column describes which LUIS Model the question is sent to to determine it's intent. The Code Written column helps us keep track of the functionality that has yet to be implemented.

## Required Functionality

|Questions											                    |LUIS Model	|Code Written	
|---------------------------------------------------|-----------|------------
|What was the last commit on `<repo>`?				      |			      |❌
|How many total commits are there on `<repo>`?		  |			      |❌
|How many files are in `<repo>`?						        |			      |❌
|How many contributors does `<repo>` have?			    |			      |❌
|When was the last commit on `<repo>`?				      |			      |❌
|What were the last `<number>` commits on `<repo>`? |			      |❌			
|What was `<user>`’s last commit on `<repo>`?			  |			      |❌
|When was `<user>`’s last commit on `<repo>`?			  |			      |❌
|How many commits has `<user>` made on `<repo>`?		|			      |❌
|What were `<user>`’s last x commits?					      |			      |❌

## Added Functionality

|Questions											        |LUIS Model	|Code Written	
|---------------------------------------|-----------|------------
|What is `<user>`'s biography?					|			      |✅
|What is `<user>`'s email address?			|			      |✅
|How many followers does `<user>` have?	|			      |✅
|How many users is `<user>` following?	|			      |✅
|Where is `<user>`'s location?					|			      |✅
|What is `<user>`'s name?								|			      |✅
