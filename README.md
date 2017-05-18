# GitBot - The GitHub Chatbot

<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-inverted-blue.png" width="200">

GitBot is a chatbot that is able to answer questions about information from GitHub.

## [How to interact with GitBot](https://github.com/nating/gitbot/wiki/Talking-to-GitBot)

You can simply send GitBot a message from one of the supported platforms.

<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-demo.gif" width="600">


## [Supported Platforms](https://github.com/nating/gitbot/wiki/Talking-to-GitBot#supported-platforms)
To start chatting to GitBot from one of these platforms right now, click the icon for the platform.  

[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/group-me-logo.png" width="60">][groupme]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/kik-logo.png" width="60">][kik]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/messenger-logo.png" width="60">][messenger]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/microsoft-teams-logo.png" width="60">][microsoft-teams]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/skype-logo.png" width="60">][skype]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/slack-logo.png" width="60">][slack]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/telegram-logo.png" width="60">][telegram]
[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/platform-logos/web-logo.png" width="60">][web]

## [How GitBot Works](https://github.com/nating/gitbot/wiki/How-GitBot-Works)

[<img src="https://github.com/nating/gitbot/blob/master/docs/assets/gitbot-explanation.png" width="700">][flow]

GitBot is a chatbot built with the [Microsoft Bot Framework][mbf] and is hosted on [Azure][azure].  

The Bot Framework connects GitBot to all of the supported messaging platforms. When a message is sent to GitBot from one of the messaging platforms, it goes to his endpoint on Azure. GitBot sends any messages he recieves to [Microsoft's Language Understanding Intelligence Service][luis] to understand the intent of the message. When GitBot knows the intent of the message, he asks for the relevant data from GitHub. When the data is recieved from GitHub, GitBot sends his response back to the user.  

[mbf]: https://dev.botframework.com/
[azure]: https://azure.microsoft.com
[luis]: https://www.microsoft.com/cognitive-services/en-us/language-understanding-intelligent-service-luis
[flow]: https://github.com/nating/gitbot/wiki/How-GitBot-Works
[groupme]: https://groupme.botframework.com/?botId=git-bot
[kik]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#kik
[messenger]: https://m.me/gitbot1738
[microsoft-teams]: https://teams.microsoft.com/l/chat/0/0?users=28:ba351a4c-6fe2-4a5f-8ffa-3dedd8132a19
[skype]: https://join.skype.com/bot/ba351a4c-6fe2-4a5f-8ffa-3dedd8132a19
[slack]: https://github.com/nating/gitbot/wiki/Talking-to-GitBot#slack
[telegram]: https://telegram.me/Git1_Bot
[web]: https://nating.github.io/gitbot
