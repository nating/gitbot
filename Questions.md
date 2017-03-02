# Questions

Here is a list of the type of questions, that we are hoping for GitBot to be able to answer.  
The LUIS Model column describes which LUIS Model the question is sent to to determine it's intent. The Code Written column helps us keep track of the functionality that has yet to be implemented.

## Repositories

|Questions											                    |LUIS Model	|LUIS Taught  |Code Written	
|---------------------------------------------------|-----------|-------------|------------
|How many contributors does `<repo>` have?			    |			      |✅			      |✅
|How many files are in `<repo>`?						        |			      |✅			      |✅
|How many watchers on `<repo>`?			                |			      |❌            |✅

## Commits

|Questions											                    |LUIS Model	|LUIS Taught  |Code Written	
|---------------------------------------------------|-----------|-------------|------------
|What was the last commit on `<repo>`?				      |			      |✅            |✅
|When was the last commit on `<repo>`?				      |			      |✅			      |✅
|How many total commits are there on `<repo>`?		  |			      |✅			      |✅
|What were the last `<number>` commits on `<repo>`? |			      |❌			      |✅		
|What was `<user>`’s last commit on `<repo>`?			  |			      |❌			      |✅
|When was `<user>`’s last commit on `<repo>`?			  |			      |❌			      |✅
|How many commits has `<user>` made on `<repo>`?		|			      |❌			      |✅
|What were `<user>`’s last x commits?					      |			      |❌			      |✅
|Who was the last person to commit on `<repo>`?			|			      |❌            |❌

## Issues

|Questions											                    |LUIS Model	|LUIS Taught  |Code Written	
|---------------------------------------------------|-----------|-------------|------------
|How many issues does `<repo>` have?                |           |❌           |❌

## Users

|Questions											                    |LUIS Model	|LUIS Taught  |Code Written	
|---------------------------------------------------|-----------|-------------|------------
|What is `<user>`'s biography?					            |			      |✅            |✅
|What is `<user>`'s email address?			            |			      |✅            |✅
|How many followers does `<user>` have?	            |			      |✅            |✅
|How many users is `<user>` following?	            |			      |✅            |✅
|Where is `<user>`'s location?					            |			      |✅            |✅
|What is `<user>`'s name?								            |			      |✅            |✅
|What `<repo>`s does this `<user>` own?			        |			      |❌            |✅
|How many repos has `<user>` starred?			          |			      |❌            |✅
