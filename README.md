# Community Crawling Engine

```
Community Crawler 2020.3.1
Build Date: Saturday, February 29, 2020
                                           ^^
      ^^      ..                                       ..
              []                                       []
            .:[]:_          ^^                       ,:[]:.
          .: :[]: :-.                             ,-: :[]: :.
        .: : :[]: : :`._                       ,.': : :[]: : :.
      .: : : :[]: : : : :-._               _,-: : : : :[]: : : :.
  _..: : : : :[]: : : : : : :-._________.-: : : : : : :[]: : : : :-._
  _:_:_:_:_:_:[]:_:_:_:_:_:_:_:_:_:_:_:_:_:_:_:_:_:_:_:[]:_:_:_:_:_:_
  !!!!!!!!!!!![]!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!![]!!!!!!!!!!!!!
  ^^^^^^^^^^^^[]^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^[]^^^^^^^^^^^^^
              []                                       []
              []                                       []
              []                                       []
   ~~^-~^_~^~/  \~^-~^~_~^-~_^~-^~_^~~-^~_~^~-~_~-^~_^/  \~^-~_~^-~~-
  ~ _~~- ~^-^~-^~~- ^~_^-^~~_ -~^_ -~_-~~^- _~~_~-^_ ~^-^~~-_^-~ ~^
     ~ ^- _~~_-  ~~ _ ~  ^~  - ~~^ _ -  ^~-  ~ _  ~~^  - ~_   - ~^_~
       ~-  ^_  ~^ -  ^~ _ - ~^~ _   _~^~-  _ ~~^ - _ ~ - _ ~~^ -
          ~^ -_ ~^^ -_ ~ _ - _ ~^~-  _~ -_   ~- _ ~^ _ -  ~ ^-
              ~^~ - _ ^ - ~~~ _ - _ ~-^ ~ __- ~_ - ~  ~^_-
                  ~ ~- ^~ -  ~^ -  ~ ^~ - ~~  ^~ - ~

Copyright (C) 2020. Commnunity Crawler Developer
E-Mail: rollrat.cse@gmail.com
Source-code: https://github.com/rollrat/com_crawler
```

Community is any form of website where an unspecified number of people can post and share their opinions.

This project collect the title and additional information of the post and build own search engine.
Provides fast web crawling using proxy and various add-ons.
We are looking for a very stable and scalable design based on Rust.

## Documents

[Development Manual](doc/development.md)

## How to use?

### Bot

### ChatBot

Custom Crawler provides `KakaoTalk(Not now)`, `Discord(Not now)` and `Telegram` chatbot features
for program controlling and information provision.

To use this feature you need to enable the `setting.json`.

```json
  "BotSettings": {
    "EnableTelegramBot": true,
    "TelegramBotAccessToken": "--- insert your access token here ---",
    "AccessIdentifierMessage": "--- insert your identifier message for audit admin ---"
  }
```

```
./com_crawler.Console --start-bot
```

You can use the following commands on the chatbot:

```
/help
/rap <Identifier Message>: Add the current user as an administrator.
/time : Return current server time.
```

### Components

### Downloader

Put the URL of the site you want to download.

```
./com_crawler.Console https://www.instagram.com/taylorswift/
```

If you want to see the download process, add `-p` option.

```
./com_crawler.Console https://www.instagram.com/taylorswift/ -p
```

If you want to format the download path, enter `--list-extractor` to see the supported options.
`%(file)s` and `%(ext)s` are provided by default.

```
./com_crawler.Console --list-extractor

...
[InstagramExtractor]
[HostName] www\.instagram\.com
[Checker] ^https?://www\.instagram\.com/(?:p\/)?(?<id>.*?)/?.*?$
[Information] Instagram extactor info
   user:             Full-name.
   account:          User-name
[Options]
   --only-images               Extract only images.
   --only-thumbnail            Extract only thumbnails.
   --include-thumbnail         Include thumbnail extracting video.
   --limit-posts               Limit read posts count. [use --limit-posts <Number of post>]
...

./com_crawler.Console -o "[%(account)s] %(user)s/%(file)s.%(ext)s" https://www.instagram.com/taylorswift/
```

### Server

Custom Crawler API Server Example: https://cc.rollrat.com/

Server Test: https://cc.rollrat.com/api/test

Server Mailing List: https://cc.rollrat.com/api/mail

It is a function that can provide various functions of custom crawler in the form of server.
You can use this feature to build an internal proxy server or to create
a bot that can gather information from outside.

```json
  "ServerSettings": {
    "WebServerPort": 7979,
    "EnableMailServer": true,
    "AuthMailUser": "support",
    "AuthMailPassword": "*"
  },
```

```
./com_crawler.Console --start-server
```

## Contribution

Welcome to any form of contribution!

If you are interested in this project or have any suggestions, feel free to open the issue!
