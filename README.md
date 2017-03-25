# GitBot - The GitHub Chatbot

<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-inverted-black.png" width="300">

GitBot is a chatbot that is able to answer questions about information from GitHub.

## [How to interact with GitBot](https://github.com/nating/gitbot/wiki/Talking-to-GitBot)

You can simply send GitBot a message from one of the supported platforms.

<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-demo.gif" width="900">


## Supported Platforms
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/web-logo.png" width="80">][web]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/email-logo.png" width="80">][email]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/messenger-logo.png" width="80">][messenger]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/skype-logo.png" width="80">][skype]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/slack-logo.png" width="80">][slack]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/sms-logo.png" width="80">][sms]

## [How GitBot Works](https://github.com/nating/gitbot/wiki/How-GitBot-Works)

<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-explanation.png" width="900">

GitBot is a chatbot built with the [Microsoft Bot Framework][mbf] and is hosted on [Azure][azure].  

The Bot Framework connects GitBot to all of the supported messaging platforms. When a message is sent to GitBot from one of the messaging platforms, it goes to his endpoint on Azure. GitBot sends any messages he recieves to [Microsoft's Language Understanding Intelligence Service][luis] to understand the intent of the message. When GitBot knows the intent of the message, he asks for the relevant data from GitHub. When the data is recieved from GitHub, GitBot sends his response back to the user.  

[mbf]: https://dev.botframework.com/
[azure]: https://azure.microsoft.com
[luis]: https://www.microsoft.com/cognitive-services/en-us/language-understanding-intelligent-service-luis
[web]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#web
[email]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#email
[messenger]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#facebook-messenger
[skype]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#skype
[slack]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#slack
[sms]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#sms
