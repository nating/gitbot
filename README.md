# GitBot - The GitHub Chatbot

<img src="https://github.com/nating/blob/master/assets/gitbot-black.png" width="100">

GitBot is a chatbot that is able to answer questions about information from GitHub.

## How to interact with GitBot

Once GitBot is published, you can simply send him a message from one of the supported platforms.

## Supported Platforms

GitBot is not yet published.

## How GitBot Works

GitBot is a chatbot built with the [Microsoft Bot Framework](https://dev.botframework.com/) and is hosted on [Azure](https://azure.microsoft.com/).  
The Bot Framework connects GitBot to all of the supported messaging platforms. When a message is sent to GitBot from one of the messaging platforms, it goes to his endpoint on Azure. GitBot sends any messages he recieves to [Microsoft's Language Understanding Intelligence Service](https://www.microsoft.com/cognitive-services/en-us/language-understanding-intelligent-service-luis) to understand the intent of the message. When GitBot knows the intent of the message, he asks for the relevant data from GitHub. When the data is recieved from GitHub, GitBot sends his response back to the user.  

<img src="https://github.com/nating/blob/master/assets/gitbot-explanation.png" width="600">
