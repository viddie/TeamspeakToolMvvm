import ts3lib, ts3defines, os, urllib, inspect, copy, collections, devtools, math, time, string, requests, json, re
from datetime import datetime, timedelta
from ts3plugin import ts3plugin
from PythonQt.QtNetwork import QNetworkAccessManager, QNetworkRequest, QNetworkReply
from PythonQt.QtCore import QUrl, Qt, QTimer
from random import *

#from tkinter import *
import subprocess, sys, pafy, traceback


class VariousUtils(ts3plugin):
    name = "VariousUtils"
    apiVersion = 22
    requestAutoload = True
    version = "1.0"
    author = "viddie"
    description = "A Plugin for various utilities such as:\n\n-NoMove\n-Various Chat Commands\n-Youtube Link lookup\n-Basic Balance system\n-Word Highlight system"
    offersConfigure = False
    commandKeyword = ""
    infoTitle = None
    menuItems = [(ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_CLIENT, 0, "Add client to NoMove list", ""),
                 (ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_CLIENT, 1, "Remove client from NoMove list", ""),
                 (ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL, 2, "Toggle RevengePoke", ""),
                 (ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL, 3, "Toggle Eco Timer", ""),
                 (ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL, 4, "Open chat with myself", ""),
                 (ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL, 5, "Test Button", "")]
    hotkeys = [("radio_1", "Quick chat #1"),
               ("radio_2", "Quick chat #2"),
               ("radio_3", "Quick chat #3"),
               ("settings_toggle_commands", "Toggle chat commands"),
               ("settings_toggle_tts", "Toggle TTS")]

    debugPlugin = True

    #Chat Commands
    chatCommandPrefix = "!"
    chatCommands = {}
    chatCommandAliases = {}

    chatCommandsEnabled = True
    immediateLinkReaction = True

    #Revenge Poke
    revengePokeEnabled = False
    revengePokeMessage = ""

    #NoMove
    noMoveFileName = "nomove.txt"

    #Magic 8 Ball
    magic8BallAnswers = ['All signs point to yes...', 'Yes!', 'My sources say nope.', 'You may rely on it.', 'Concentrate and ask again...',
                         'Outlook not so good...', 'It is decidedly so!', 'Better not tell you.', 'Very doubtful.', 'Yes - Definitely!', 'It is certain!',
                         'Most likely.', 'Ask again later.', 'No!', 'Looks good.', 'Don\'t count on it.', 'No you son of a bitch :^).', 'Without a doubt', 'As I see it yes',
                         'Signs point to yes', 'Only if you are lucky.', 'I cannot predict this right now.', 'My reply is no']
    magic8BallSavedAnswers = {}

    #Chat highlight
    highlightEnabled = False
    highlightFileName = "hightlightKeywords.txt"
    highlightSaveFileName = "highlightSave.txt"

    #Auto AFK message
    afkEnabled = True
    afkPrefix = "[afk]"
    afkMessage = "Anything you want to tell someone while being afk"
    afkBotTag = "[Bot Response]:"

    #Door
    doorEnabled = True
    doorKickMessage = "Hat das Haus verlassen."
    doorChannelName = "Tür"

    #Economy system
    ecoBalances = {}
    ecoTickTimer = None
    ecoTickTimeInSeconds = 20 * 60      #10 minutes * 60 (seconds / minute) = 600 seconds
    ecoPointLimit = 100
    ecoFileName = "ecoBalance.txt"
    ecoEnabledForServerUID = "uVPNcN/GeVSGVZmiVuSeQj5TI6M="
    ecoBlacklistFileName = "ecoBlacklist.txt"
    ecoStartTime = None
    ecoWhitelisted = -1
    ecoExcluded = 0
    ecoBlacklisted = 1
    ecoExcludedMaxSendAmount = 20

    #Youtube constants
    YT_TITLE = 0
    YT_CHANNEL = 1
    YT_VIEWS = 2
    YT_DESCRIPTION = 3
    YT_TIME_IN_SECONDS = 4
    YT_DATE = 5
    YT_LIKES = 6
    YT_DISLIKES = 7

    ytResponseTarget = []

    #Twitch constants
    TWITCH_TITLE = 0
    TWITCH_NAME_AND_GAME = 1

    twitchResponseTarget = []

    chromeSpecialChars = {"&#39;" : "'", "&quot;" : "\"", "&lt;" : "<", "&gt;" : ">", "&amp;" : "&", "\\u0026" : "&"}

    #Upload constants
    UPLOAD_URL = "https://fu.vi-home.de/upload?key={}"
    DIRECT_UPLOAD_URL = "https://fu.vi-home.de/process_upload.php"
    DOWNLOAD_URL = "https://fu.vi-home.de/view.php?fid={}"
    DIRECT_URL = "https://fu.vi-home.de/f/{}"
    uploadAPIKeyPublic = "3QfQd4121Uz2LQZConhUu2lBWRblRwFT"
    uploadAPIKeyPrivate = "sHf3FfOL9GUdAWbBJ0PoTGermZUdpmeq"
    previousToken = ""

    #Auto reconnect
    rcTabs = {}
    rcEnabled = True
    rcRevengeKickEnabled = False
    rcRevengeKickMessage = ""
    rcKickerInfo = []
    rcDelay = 0
    rcSchid = 0

    #AntiCorruption
    acEnabled = True
    acAdminGroupID = 0
    acPrivKey = ""

    #Google
    nwmc = None
    cmdevent = {}

    #Config
    configFileName = "config.txt"
    initConfigVar = "No"
    configLoadVars = {}

    #SingleShotParams
    singleShotParams = {}
    tokenTimers = {}
    pokeTimers = {}
    textTimers = {}
    cooldownTimers = {}
    timerStorages = []

    #Tokens
    tokenEnabled = True
    tokenListTokens = {"cKick" : ["Channel Kick", 5, "5s"],
                 "sKick" : ["Server Kick", 30, "30s"],
                 "shoot" : ["Tries to shoot someone :)", 6, "5s"],
                 "clear" : ["Clears the chat", 1, "15s"],
                 "gag1" : ["Gag for 10s", 7, "10s", "10s"],
                 "gag2" : ["Gag for 20s", 13, "20s", "20s"],
                 "gag3" : ["Gag for 30s", 17, "30s", "30s"],
                 "sticky1" : ["Sticky for 30s", 5, "30s", "30s"],
                 "sticky2" : ["Sticky for 1m", 10, "1m", "1m"],
                 "sticky3" : ["Sticky for 5m", 17, "5m", "5m"],
                 "mute1" : ["Mute for 20s(WIP)", 999, "20s", "20s"],
                 "mute2" : ["Mute for 40s(WIP)", 999, "40s", "40s"],
                 "mute3" : ["Mute for 1m20s(WIP)", 999, "1m20s", "1m20s"],
                 "banPerm" : ["Permanent ban", 101, "20s"],
                 "poke1" : ["Annoying poke 8x1 per second", 7, "8s", 8],
                 "poke2" : ["Annoying poke 20x1 per second", 15, "20s", 20],
                 "poke3" : ["Annoying poke 50x1 per second", 32, "50s", 50]}
    tokenParameters = {"cKick" : [1, "target"],
                       "sKick" : [1, "target"],
                       "shoot" : [1, "target"],
                       "clear" : [0, "none"],
                       "gag1" : [1, "target"],
                       "gag2" : [1, "target"],
                       "gag3" : [1, "target"],
                       "sticky1" : [1, "target"],
                       "sticky2" : [1, "target"],
                       "sticky3" : [1, "target"],
                       "mute1" : [1, "target"],
                       "mute2" : [1, "target"],
                       "mute3" : [1, "target"],
                       "banPerm" : [1, "target"],
                       "poke1" : [1, "target"],
                       "poke2" : [1, "target"],
                       "poke3" : [1, "target"]}
    tokenGagGroupID = 0
    tokenStickyGroupID = 0
    tokenMuteGroupID = 0
    tokenStickyClientChannels = {}
    tokenOperationParams = []

    #Priv
    privUserLevel = 0
    privModLevel = 1
    privAdminLevel = 2
    privMyLevel = 3
    privFileName = "privileges.txt"

    #Random links
    apiKey = "9581273750602189"

    #Betting
    betEnabled = True
    betMinAmount = 1
    betMaxAmount = 4
    betOpenTimers = {}#{timer : [senderUID, targetUID]}
    betOngoingTimers = {}#{timer : [senderUID, targetUID, correctNum, senderNum, targetNum, winAmount]}

    #Save/load TS Layout to/from file
    tssStarted = False
    tssServerGroups = {}#{serverGroupID : {permissionID : [permissionValue, permissionNegated, permissionSkip]}}
    tssChannelGroups = {}#{channelGroupID : {permissionID : [permissionValue, permissionNegated, permissionSkip]}}
    tssChannels = {}#{channelID : [parentName, name, topic, description, password, maxclients, permanent/semi/default, talkpower]}

    #Text to Speech
    ttsEnabled = True
    ttsVolume = 0.8
    ttsMaxChars = 256
    ttsCharsPerPoint = 10
    ttsCDPerChar = 1
    ttsUnicodeIncrease = 0.4
    ttsMinCD = 10
    ttsMaxCD = 100
    ttsGlobalOutput = -1
    ttsLocalOutput = -1
    ttsUserCD = {}


    #Options
    optionsDict = {}
    optionsColorBoolTrue = "#00aa00"
    optionsColorBoolFalse = "#ff0000"
    optionsColorOther = "#000099"


    #Roulette
    roulEnabled = True
    roulWinMultiplier = 2
    roulWinChancePercent = 50
    roulCooldown = "30m"
    roulStatsFile = "roulette_stats.txt"
    roulSpecialWinSound = ""
    roulCashSound = "cash"


    #Quick chat
    quickChatEnabled = True
    quickChat1 = "Yes!"
    quickChat2 = "No!"
    quickChat3 = "Fuck you!"


    #Playsounds
    psEnabled = True
    psSounds = {} #{displayName : [fileName, cost]}
    psCooldown = "10s"
    psSoundsFileName = "playsounds_save.txt"
    psProcessStorage = None
    psSoundsPerPage = 15
    psDefaultVolume = 1.0
    psDefaultSpeed = 1.0


    #Dailies
    dailyEnabled = True
    dailyCooldown = "07:00"
    dailyUseCooldownAsResetTime = True
    dailyReward = 9
    dailySaveFile = "daily_cooldowns.txt"


    #Rewards
    rewardsSaveFile = "rewards_save.txt"
    rewardsEnabled = True


    #FloodPrevention
    fpEnabled = False
    fpMaxPokesPerMinute = 6
    fpMaxPrivatePerMinute = -1
    fpMaxCommandsPerMinute = -1
    fpActions = {"poke" : {}, "private_chat" : {}, "command" : {}}
    fpSupressed = {"poke" : {}, "private_chat" : {}, "command" : {}}


    #Stats
    statsData = {}

    def __init__(self):
        self.debug("-------------Launched------------")

        self.chatCommands["coinflip"] = ["coinflip [amount]", "Flips a coin [amount] times, default is once.", self.privUserLevel]
        self.chatCommands["roll"] = ["roll [from] [<to>]", "Rolls a random number between [from] and [to], default is 1 and 100.", self.privUserLevel]
        self.chatCommands["help"] = ["help [command]", "Gives more detailed information about a command.", self.privUserLevel]
        self.chatCommands["n"] = ["n", "CmonBruh", self.privUserLevel]
        self.chatCommands["8ball"] = ["8ball <your question>", "Asks the magic 8ball a question.", self.privUserLevel]
        self.chatCommands["russianroulette"] = ["russianroulette", "Are you lucky? 1 in 7 chance of getting kicked", self.privUserLevel]
        self.chatCommands["suicide"] = ["suicide", "Don't do it! Kapp", self.privUserLevel]
        self.chatCommands["highlight"] = ["highlight <list|add|remove>", "Add, remove or list highlighted keywords", self.privMyLevel]
        self.chatCommands["balance"] = ["balance [send/name] [<name>] [<amount>]", "Get your own balance or send balance to others", self.privUserLevel]
        self.chatCommands["excludeme"] = ["excludeme", "Excludes yourself from the economy system", self.privMyLevel]
        self.chatCommands["blacklist"] = ["blacklist [<name>|<clientUID>]", "Blacklists certain UIDs or clients from the economy system", self.privAdminLevel]
        self.chatCommands["exclude"] = ["exclude [<name>|<clientUID>]", "Excludes certain UIDs or clients from the economy system", self.privModLevel]
        self.chatCommands["whitelist"] = ["whitelist [<name>|<clientUID>]", "Whitelists certain UIDs or clients from the economy system", self.privAdminLevel]
        self.chatCommands["token"] = ["token [unstuck|tokenName] [<targetName>]", "Uses the specified token, when you have enough balance for it", self.privUserLevel]
        self.chatCommands["setbalance"] = ["setbalance <namePart> [~]<amount>", "Sets someone's balance. '~' for relative setting", self.privAdminLevel]
        self.chatCommands["youtube"] = ["youtube <url|watch id>", "Return basic information about the video", self.privUserLevel]
        self.chatCommands["twitchclip"] = ["twitchclip <url>", "Return basic information about the clip", self.privUserLevel]
        self.chatCommands["setvariable"] = ["setvariable <variable> <value>", "Sets a variable. \"\" for strings!", self.privMyLevel]
        self.chatCommands["varvalue"] = ["varvalue <variable> [variable...]", "Gets the value of a variable. Can show multiple variables at once", self.privAdminLevel]
        self.chatCommands["test"] = ["test", "Test command.", self.privMyLevel]
        self.chatCommands["playsound"] = ["playsound [name]", "Plays the desired sound", self.privUserLevel]
        self.chatCommands["google"] = ["google <anything>", "Returns googles first few hits for the specified term", self.privUserLevel]
        self.chatCommands["timed"] = ["timed <list|cancel|timedelta> [<cmd>] [args]", "Executes a command at a different time, cancels or lists timers. Example timedelta: \"5m17s4h\"", self.privUserLevel]
        self.chatCommands["privilege"] = ["priv <user> <level|user|mod|admin>", "Sets privilege levels for given users", self.privMyLevel]
        self.chatCommands["link"] = ["link [longID]", "Returns a random link or a specific link for a given longID", self.privUserLevel]
        self.chatCommands["bet"] = ["bet [opponent's name] [points (1-4)]", "Challenges someone else to a bet", self.privUserLevel]
        self.chatCommands["nowplaying"] = ["nowplaying [pp|stats|detailed]", "To see what I am currently playing in osu!", self.privUserLevel]
        self.chatCommands["tts"] = ["tts <message>", "Text-to-speech a given message!", self.privUserLevel]
        self.chatCommands["options"] = ["options [varName/moduleName] [<newValue>...]", "List/Set options", self.privMyLevel]
        self.chatCommands["roulette"] = ["roulette <points[%] or all or stats> [<name>...]", "Gamble your points away!", self.privUserLevel]
        self.chatCommands["paypal"] = ["paypal [pointsToBuy]", "Buy new points via paypal!", self.privUserLevel]
        self.chatCommands["tick"] = ["tick", "To see when the next point arrives.", self.privUserLevel]
        self.chatCommands["daily"] = ["daily", "To get your daily bonus.", self.privUserLevel]
        self.chatCommands["reward"] = ["reward <code>", "Redeem a reward code.", self.privUserLevel]
        self.chatCommands["makereward"] = ["makereward <type> <expireTime> <amount>", "Create a reward code.", self.privMyLevel]
        self.chatCommands["watch2gether"] = ["watch2gether", "Create a watch2gether room.", self.privMyLevel]
        self.chatCommands["floodprevent"] = ["floodprevent <poke|command|private_chat>", "Checks out the flood prevention stats", self.privMyLevel]
        self.chatCommands["wakemeup"] = ["wakemeup", "Pokes you. Used best with a timed event.", self.privUserLevel]
        self.chatCommands["afk"] = ["afk [message]", "Marks you as afk. Optionally sends a message to whoever pokes / pn's you", self.privMyLevel]
        self.chatCommands["todo"] = ["todo [message|remove] [<id>]", "List, add or remove a TO-DO", self.privMyLevel]
        self.chatCommands["command"] = ["command [add|remove|list] [<cmd>]", "List, add or remove a command", self.privUserLevel]
        self.chatCommands["upload"] = ["upload [path] [<password>]", "Returns the upload url or uploads a file directly", self.privUserLevel]
        self.chatCommands["teams"] = ["team <player1> <player2...>", "Splits the players into equal teams", self.privUserLevel]
        self.chatCommands["stats"] = ["stats <module> <field>", "Some stats", self.privUserLevel]

        self.chatCommandAliases["n"] = ["nig", "nigger", "lirik", "nignog", "black", "cmonbruh", "neckers", "schwarz", "pewdiepie", "hyperbruh"]
        self.chatCommandAliases["youtube"] = ["yt"]
        self.chatCommandAliases["twitchclip"] = ["twitch", "clip"]
        self.chatCommandAliases["setvariable"] = ["setvar"]
        self.chatCommandAliases["varvalue"] = ["varval"]
        self.chatCommandAliases["google"] = ["g"]
        self.chatCommandAliases["suicide"] = ["kickme", "killme"]
        self.chatCommandAliases["privilege"] = ["priv"]
        self.chatCommandAliases["bet"] = ["b"]
        self.chatCommandAliases["nowplaying"] = ["np"]
        self.chatCommandAliases["roulette"] = ["roul"]
        self.chatCommandAliases["russianroulette"] = ["rr"]
        self.chatCommandAliases["playsound"] = ["ps"]
        self.chatCommandAliases["watch2gether"] = ["w2g"]
        self.chatCommandAliases["floodprevent"] = ["fp"]
        self.chatCommandAliases["wakemeup"] = ["wmu"]
        self.chatCommandAliases["upload"] = ["ul"]
        self.chatCommandAliases["teams"] = ["team"]


        self.timerStorages = [self.tokenTimers, self.singleShotParams, self.pokeTimers, self.textTimers, self.cooldownTimers, self.betOpenTimers, self.betOngoingTimers]



        self.prepareConfigVar()
        self.initConfig()
        self.loadConfig()

        self.prepareOptionsVar()
        self.ecoLoadFromFile()

        self.ecoTickTimer = QTimer()
        self.ecoTickTimer.timeout.connect(self.ecoTick)
        self.ecoTickTimer.start(self.ecoTickTimeInSeconds * 1000)

        self.ecoStartTime = datetime.now()


        self.loadPlaysounds()
        self.loadStats()


    def stop(self):
        self.saveConfig()

        #Eco
        self.ecoSaveToFile()
        self.ecoTickTimer.stop()

        self.savePlaysounds()
        self.saveStats()

        #Timers
        for timerStorage in self.timerStorages:
            for timer in timerStorage:
                if(timer.isActive()):
                    listParams = timerStorage[timer]
                    timer.stop()

                    method = listParams[0]
                    cmdParameters = listParams[1]#schid, targetMode, targetID, senderName
                    schid, targetMode, targetID, senderIdentifier = cmdParameters[0], cmdParameters[1], cmdParameters[2], cmdParameters[3]
                    parameters = listParams[2]
                    ignoreOfflineCheck = listParams[3]

                    if(ignoreOfflineCheck == False):
                        clientID = self.getClientIDByUID(schid, senderIdentifier)
                    else:
                        clientID = self.getClientIDByName(schid, senderIdentifier)

                    if(clientID == -1):
                        continue
                    else:
                        self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, "[NOTICE] The plugin was reloaded, thus your timer for [b]'{}'[/b] was killed.".format(method.__name__), clientID)




    def onMenuItemEvent(self, schid, atype, menuItemID, selectedItemID):
        if(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_CLIENT and menuItemID == 0):
            (err, clientUID) = ts3lib.getClientVariableAsString(schid, selectedItemID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
            self.addNoMove(clientUID)

        elif(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_CLIENT and menuItemID == 1):
            (err, clientUID) = ts3lib.getClientVariableAsString(schid, selectedItemID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
            self.removeNoMove(clientUID)

        elif(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL and menuItemID == 2):
            self.toggleRevengePoke()

        elif(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL and menuItemID == 3):
            if self.ecoTickTimer == None:
                self.ecoTickTimer = QTimer()
                self.ecoTickTimer.timeout.connect(self.ecoTick)

            if self.ecoTickTimer.isActive():
                self.ecoTickTimer.stop()
                self.ecoTickTimer = None
                ts3lib.printMessageToCurrentTab('Timer stopped!')
            else:
                self.ecoTickTimer.start(self.ecoTickTimeInSeconds * 1000)
                ts3lib.printMessageToCurrentTab('Timer restarted!')

        elif(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL and menuItemID == 4):
            ts3lib.requestSendPrivateTextMsg(schid, ".", self.getMyID(schid))

        elif(atype == ts3defines.PluginMenuType.PLUGIN_MENU_TYPE_GLOBAL and menuItemID == 5):
            self.debug("channel ID: {}".format(ts3lib.getChannelOfClient(schid, self.getMyID(schid))))
            self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL, "Some shit", 9723492)




#---------------------TS SAVE TO LAYOUT---------------------------------------------------------
    def onServerGroupListEvent(self, schid, serverGroupID, name, atype, iconID, saveDB):
        #self.debug("ServerGroup '{}' with ID '{}' and ATYPE '{}'".format(name, serverGroupID, atype))
        if(self.tssStarted != True or atype != 1):
            return

        self.tssServerGroups[serverGroupID] = [name, {}]
        failed = ts3lib.requestServerGroupPermList(schid, serverGroupID)

        if(failed != ts3defines.ERROR_ok):
            self.debug("Failed to request server group permissions for group '{}'".format(name))
            self.tssStarted = False

    def onServerGroupPermListEvent(self, schid, serverGroupID, permissionID, permissionValue, permissionNegated, permissionSkip):
        if(self.tssStarted != True or serverGroupID not in self.tssServerGroups):
            return

        self.tssServerGroups[serverGroupID][1][permissionID] = [permissionValue, permissionNegated, permissionSkip]


    def onServerGroupPermListFinishedEvent(self, schid, serverGroupID):
        if(self.tssStarted != True):
            return
        self.debug("Done saving server group ({})".format(serverGroupID))

        for key in self.tssServerGroups:
            if(len(self.tssServerGroups[key][1]) == 0):
                return

        self.debug("\nFinished all server groups, starting channel groups...\n")
        ts3lib.requestChannelGroupList(schid)


    #---Channel Groups---#

    def onChannelGroupListEvent(self, schid, channelGroupID, name, atype, iconID, saveDB):
        #self.debug("ChannelGroup '{}' with ID '{}' and ATYPE '{}'".format(name, channelGroupID, atype))
        if(self.tssStarted != True or atype != 1):
            return

        self.tssChannelGroups[channelGroupID] = [name, {}]
        failed = ts3lib.requestChannelGroupPermList(schid, channelGroupID)

        if(failed != ts3defines.ERROR_ok):
            self.debug("Failed to request channel group permissions for group '{}'".format(name))
            self.tssStarted = False

    def onChannelGroupPermListEvent(self, schid, channelGroupID, permissionID, permissionValue, permissionNegated, permissionSkip):
        if(self.tssStarted != True or channelGroupID not in self.tssChannelGroups):
            return

        self.tssChannelGroups[channelGroupID][1][permissionID] = [permissionValue, permissionNegated, permissionSkip]


    def onChannelGroupPermListFinishedEvent(self, schid, channelGroupID):
        if(self.tssStarted != True):
            return
        self.debug("Done saving channel group ({})".format(channelGroupID))

        for key in self.tssChannelGroups:
            if(len(self.tssChannelGroups[key][1]) == 0):
                return

        self.tssStarted = False
        self.debug("\nFinished all groups, starting channels...\n")

        #Channels -> {channelID : [parentName, name, password, topic, description, maxclients, permanent/semi/default, talkpower]}

        channelList = ts3lib.getChannelList(schid)[1]

        for channelID in channelList:
            info = []
            (error, cpath, password) = ts3lib.getChannelConnectInfo(schid, channelID, 1024)
            parents = cpath.replace("\\/", "{fwsl}").split("/")
            #self.debug("parents '{}'".format(parents))
            if(len(parents) == 1):
                parentName = ""
                name = parents[0].replace("{fwsl}", "/")
            elif(len(parents) == 2):
                parentName = parents[-2].replace("{fwsl}", "/")
                name = parents[len(parents)-1].replace("{fwsl}", "/")

            (err, topic) = ts3lib.getChannelVariableAsString(schid, channelID, ts3defines.ChannelProperties.CHANNEL_TOPIC)
            (err, description) = ts3lib.getChannelVariableAsString(schid, channelID, ts3defines.ChannelProperties.CHANNEL_DESCRIPTION)
            (err, maxclients) = ts3lib.getChannelVariableAsInt(schid, channelID, ts3defines.ChannelProperties.CHANNEL_MAXCLIENTS)

            (err, permanent) = ts3lib.getChannelVariableAsInt(schid, channelID, ts3defines.ChannelProperties.CHANNEL_FLAG_PERMANENT)
            (err, semipermanent) = ts3lib.getChannelVariableAsInt(schid, channelID, ts3defines.ChannelProperties.CHANNEL_FLAG_SEMI_PERMANENT)
            (err, default) = ts3lib.getChannelVariableAsInt(schid, channelID, ts3defines.ChannelProperties.CHANNEL_FLAG_DEFAULT)

            channelType = 0 if default == 1 else 1 if permanent == 1 else 2 if semipermanent == 1 else -1

            (err, talkpower) = ts3lib.getChannelVariableAsInt(schid, channelID, ts3defines.ChannelPropertiesRare.CHANNEL_NEEDED_TALK_POWER)

            info = [parentName, name, password, topic, description, maxclients, channelType, talkpower]

            self.debug("Done saving channel '{}'".format(info))
            self.tssChannels[channelID] = info

        self.debug("\n Finished all channels, starting to save layout to file...\n")
        self.saveTSLayout()


    #{serverGroupID : [name, {permissionID : [permissionValue, permissionNegated, permissionSkip]}]}
    #{channelGroupID : [name, {permissionID : [permissionValue, permissionNegated, permissionSkip]}]}
    #{channelID : [parentName, name, topic, description, password, maxclients, permanent/semi/default, talkpower]}


    def saveTSLayout(self):
        file = self.getFile("tsLayout.txt", "w", "utf-8")
        data = {
            "serverGroups": self.tssServerGroups,
            "channelGroups": self.tssChannelGroups,
            "channels": self.tssChannels
        }
        json.dump(data, file)
        # file.write("ServerGroups\n")
        #
        # for serverGroupID in self.tssServerGroups:
        #     (name, permissionDict) = self.tssServerGroups[serverGroupID]
        #     saveString = "[\"{}\",{}]\n".format(name, permissionDict)
        #     file.write(saveString)
        #
        # file.write("ChannelGroups\n")
        #
        # for channelGroupID in self.tssChannelGroups:
        #     (name, permissionDict) = self.tssChannelGroups[channelGroupID]
        #     saveString = "[\"{}\",{}]\n".format(name, permissionDict)
        #     file.write(saveString)
        #
        # file.write("Channels\n")
        #
        # for channelID in self.tssChannels:
        #     (parentName, name, topic, description, password, maxclients, permanent, talkpower) = self.tssChannels[channelID]
        #
        #     parentName = self.tssRemoveSpecialChars(parentName)
        #     name = self.tssRemoveSpecialChars(name)
        #     topic = self.tssRemoveSpecialChars(topic)
        #     description = self.tssRemoveSpecialChars(description)
        #     password = self.tssRemoveSpecialChars(password)
        #
        #     saveString = "[\"{}\",\"{}\",\"{}\",\"{}\",\"{}\",{},{},{}]\n".format(parentName, name, topic, description, password, maxclients, permanent, talkpower)
        #     file.write(saveString)


        file.close()
        self.debug("\nLayout saved.")



    def tssRemoveSpecialChars(self, string):
        string = string.replace("$", "dollarsign")
        string = string.replace("\n", "$nl$")
        string = string.replace("\t", "$tab$")
        string = string.replace("\\", "$bs$")
        string = string.replace("'", "$hk$")
        string = string.replace("\"", "$gf$")
        string = string.replace("[", "$oab$")
        string = string.replace("]", "$cab$")
        string = string.replace("{", "$ocb$")
        string = string.replace("}", "$ccb$")
        return string

    def tssAddSpecialChars(self, string):
        string = string.replace("$nl$", "\n")
        string = string.replace("$tab$", "\t")
        string = string.replace("$bs$", "\\")
        string = string.replace("$hk$", "'")
        string = string.replace("$gf$", "\"")
        string = string.replace("$oab$", "[")
        string = string.replace("$cab$", "]")
        string = string.replace("$ocb$", "{")
        string = string.replace("$ccb$", "}")
        string = string.replace("dollarsign", "$")
        return string


#-----------------------------------------------------------------------------------------------







    def onServerGroupClientDeletedEvent(self, schid, clientID, clientName, clientUID, serverGroupID, invokerID, invokerName, invokerUID):
        if self.acEnabled == False: return
        if clientID != self.getMyID(schid): return
        if clientID == invokerID: return
        if serverGroupID != self.acAdminGroupID: return

        if(self.acPrivKey == ""):
            self.debug("There is no saved priv key to use...")
            return

        error = ts3lib.privilegeKeyUse(schid, self.acPrivKey)
        if(error == ts3defines.ERROR_privilege_key_invalid):
            self.debug("That privilege key didn't work...")
        else:
            error = ts3lib.requestClientKickFromServer(schid, invokerID, "Versuchs garnicht erst...")
            self.debug("YO, remember to add a new priv key!")


    def onClientMoveMovedEvent(self, schid, clientID, oldChannelID, newChannelID, visibility, moverID, moverName, moverUniqueIdentifier, moveMessage):
        channelName = self.getChannelNameByID(schid, newChannelID)
        if(channelName == self.doorChannelName and self.doorEnabled == True):
            ts3lib.requestClientKickFromServer(schid, clientID, self.doorKickMessage)
            return
        (err, clientUID) = ts3lib.getClientVariableAsString(schid, selectedItemID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
        if(self.isNoMove(clientUID) and self.isMe(schid, clientID)):
            ts3lib.requestClientMove(schid, clientID, oldChannelID, "")

    def onClientMoveEvent(self, schid, clientID, oldChannelID, newChannelID, visibility, moveMessage):
        channelName = self.getChannelNameByID(schid, newChannelID)
        if(channelName == self.doorChannelName and self.doorEnabled == True):
            ts3lib.requestClientKickFromServer(schid, clientID, self.doorKickMessage)
            return
        if clientID != self.rcTabs[schid]["clid"]:
            return
        # (err, pw) = ts3lib.getChannelVariable(schid, newChannelID, ts3defines.ChannelProperties.CHANNEL_FLAG_PASSWORD)
        (err, self.rcTabs[schid]["cpath"], self.rcTabs[schid]["cpw"]) = ts3lib.getChannelConnectInfo(schid, newChannelID)


    def onClientPokeEvent(self, schid, fromClientID, pokerName, pokerUniqueIdentity, message, ffIgnored):
        if(self.floodPrevent(pokerUniqueIdentity, "poke") == 1):
            return 1

        if(self.revengePokeEnabled):
            self.debug("Poke suppressed from {}".format(pokerName))
            ts3lib.requestClientPoke(schid, fromClientID, self.revengePokeMessage)
            return 1        #Ignore poke

        if(self.amAfk(schid)):
            self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, "{}{}".format(self.afkBotTag, self.afkMessage), fromClientID)


    def onConnectStatusChangeEvent(self, schid, newStatus, errorNumber):
        if newStatus == ts3defines.ConnectStatus.STATUS_CONNECTION_ESTABLISHED:
            self.saveTab(schid)
            if(len(self.rcKickerInfo) != 0):
                if(self.rcEnabled == False):
                    return
                kickerID = self.rcKickerInfo[0]
                kickerName = self.rcKickerInfo[1]
                kickerUniqueIdentifier = self.rcKickerInfo[2]
                kickMessage = self.rcKickerInfo[3]
                if(kickMessage == ""):
                    ts3lib.requestClientKickFromServer(schid, kickerID, "??")
                else:
                    kickMessage = self.rcRevengeKickMessage.replace("!kickmsg!", kickMessage)
                    ts3lib.requestClientKickFromServer(schid, kickerID, kickMessage)
                self.rcKickerInfo = []


    def onClientKickFromServerEvent(self, schid, clientID, oldChannelID, newChannelID, visibility, kickerID, kickerName, kickerUniqueIdentifier, kickMessage):
        if kickerID == clientID: return
        if clientID != self.rcTabs[schid]["clid"]: return
        if self.rcEnabled == False: return
        if schid not in self.rcTabs: return
        if self.rcRevengeKickEnabled:
            self.rcKickerInfo = [kickerID, kickerName, kickerUniqueIdentifier, kickMessage]
        if self.rcDelay > 0:
            self.rcSchid = schid
            QTimer.singleShot(self.rcDelay, self.reconnect)
        else:
            self.reconnect(schid)



    def onTextMessageEvent(self, schid, targetMode, toID, fromID, fromName, fromUniqueIdentifier, message, ffIgnored):
        #My channel ID
        myChannelID = self.getMyChannelID(schid)
        fromUID = self.getClientUIDByID(schid, fromID)

        #Return if the message is not a channel or private message
        if(targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT
           and targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL):
            return

        #Set the target of the return message to the channelID or senderID
        if(targetMode == ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT):
            if(fromID == self.getMyID(schid)):
                targetID = toID
            else:
                targetID = fromID
                if(self.floodPrevent(fromUniqueIdentifier, "private_chat") == 1):
                    return 1

        elif(targetMode == ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL):
            targetID = myChannelID

        #Return if the message does not start with the required prefix
        if(message.startswith(self.chatCommandPrefix) == False or message == self.chatCommandPrefix):

            if(message.startswith(" !")):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Fuck off with your spaces, [B]Panther[/B][/color]", targetID)

            #HIGHLIGHTER -> Check for defined keywords and mark them
            if(self.highlightEnabled == True):
                self.filterHighlightWords(schid, targetMode, targetID, fromName, message)

            #Youtube link automatic reply
            if(self.immediateLinkReaction == True and ((message.startswith("[URL]") and message.endswith("[/URL]") and "youtube" in message and "watch?v" in message and not "music.youtube" in message) or (message.startswith("[URL]") and message.endswith("[/URL]") and "youtu.be" in message))):
                self.cmdYoutube(schid, targetMode, targetID, fromName, [message], True)

            if(self.immediateLinkReaction == True and (message.startswith("[URL]") and message.endswith("[/URL]") and "clips.twitch.tv" in message)):
                self.cmdTwitchclip(schid, targetMode, targetID, fromName, [message], True)

            elif(targetMode == ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT and not self.isMe(schid, fromID) and self.amAfk(schid)):
                if(message.startswith(self.afkBotTag) == False):
                    self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, "{}: {}".format(self.afkBotTag, self.afkMessage), targetID)

            #self.directAnswer(schid, targetMode, targetID, fromName, message)
            return

        if(self.chatCommandsEnabled == False):
            if((not message.startswith("{}varval".format(self.chatCommandPrefix)) or not self.isMe(schid, fromID)) and (not message.startswith("{}setvar".format(self.chatCommandPrefix)) or not self.isMe(schid, fromID))):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Commands are currently disabled.[/color]", targetID)
                return

        #self.debug("Got message '{}' from '{}' to target '{}'. myChannelID: '{}'. From ID '{}'".format(message, fromName, targetMode, myChannelID, fromID))

        #Return and answer when the client is blocked
        if(ffIgnored):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Fuck you, I'm not talking to you \"{}\" >:([/color]".format(fromName), targetID)
            return

        #Return and answer if someone wrote "<prefix> " in order to throw of the script
        if(message == self.chatCommandPrefix+" "):
            self.sendTextMessage(schid, targetMode, "Are you stupid, [B]{}[/B]?".format(fromName), targetID)
            return

        if((not self.isMe(schid, fromID) and self.privGetClientLevel(fromUniqueIdentifier) < self.privModLevel) and self.floodPrevent(fromUniqueIdentifier, "command") == 1):
            self.sendTextMessage(schid, targetMode, "[color=#bb0000]You are sending too many requests, [b]'{}'[/b], calm down a little![/color]".format(fromName), targetID)
            return

        #Split the entered message into its parts, command and parameters
        fullCommand = self.parseCommand(message)
        command = fullCommand[0].lower()

        if(len(fullCommand) > 1):
            parameters = fullCommand[1:]
        else:
            parameters = []

        indices = []
        for i in range(0, len(parameters)):
            if(parameters[i] == ""):
                indices.append(i)

        for i in indices:
            parameters.pop(i)

        i = 0

        while True:
            if(i >= len(parameters)):
                break
            param = parameters[i]

            if(param.startswith("\"")):
                startAt = i
                while not parameters[i].endswith("\""):
                    i += 1
                    if(i == len(parameters)):
                        self.sendTextMessage(schid, targetMode, "[color=#bb0000]Syntax error: Found opening \" but no closing \", [b]@'{}'[/b][/color]".format(fromName), targetID)
                        return
                joinedParam = " ".join(parameters[startAt:i+1])
                joinedParam = joinedParam[1:-1]

                for index in range(startAt, i+1):
                    parameters.pop(startAt)

                parameters.insert(startAt, joinedParam)

            i += 1

        #self.debug("all parameters: '{}'".format(parameters))




        #self.debug("Bevor: command: '{}', Length list parameters: '{}'".format(command, len(parameters)))
        #Check if the entered command exists, otherwise check if the entered command is an alias from a valid command, otherwise return error message
        if(self.isCommand(command) == False):
            if(self.getCommandFromAlias(command) == -1):
                self.sendTextMessage(schid, targetMode, "Sorry [B]{}[/B], that command doesn't exist. Type \"{}help\" for all available commands.".format(fromName, self.chatCommandPrefix), targetID)
                return
            else:
                command = self.getCommandFromAlias(command)

        if(not self.canClientUseCommand(fromUID, command)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], you can't use that command.(1)[/color]".format(fromName), targetID)
            return


        commandCapitaled = command[0:1].upper() + command[1:]
        self.statsIncrement("All", "Commands used", 1, 0)

        getattr(self, "cmd{}".format(commandCapitaled))(schid, targetMode, targetID, fromName, parameters)

    def onHotkeyEvent(self, keyword):
        schid = self.getCurrentSCHID()
        myChannelID = self.getMyChannelID(schid)
        targetMode = ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL

        if(keyword.startswith("radio")):
            if(keyword == "radio_1"):
                self.sendTextMessage(schid, targetMode, self.quickChat1, myChannelID)
            elif(keyword == "radio_2"):
                self.sendTextMessage(schid, targetMode, self.quickChat2, myChannelID)
            elif(keyword == "radio_3"):
                self.sendTextMessage(schid, targetMode, self.quickChat3, myChannelID)

        elif(keyword.startswith("settings")):#def cmdOptions(self, schid, targetMode, targetID, senderName, parameters):
            if(keyword == "settings_toggle_commands"):
                self.cmdOptions(schid, targetMode, myChannelID, self.getMyNameByID(schid), ["ChatCommands", "disable" if self.chatCommandsEnabled else "enable"])
            elif(keyword == "settings_toggle_tts"):
                self.cmdOptions(schid, targetMode, myChannelID, self.getMyNameByID(schid), ["TTS", "disable" if self.ttsEnabled else "enable"])





    def addChatCommand(self, command, usage, explanation):
        self.chatCommands[command] = [usage, explanation]

    def addCommandAlias(self, command, *alias):
        self.chatCommandAliases[command] = alias

    def parseCommand(self, message):
        message = message[len(self.chatCommandPrefix):]
        return message.split(" ")

    def isCommand(self, command):
        if(command in self.chatCommands):
            return True
        return False

    def getCommandUsage(self, command):
        for key in self.chatCommands:
            if(key == command):
                return self.chatCommandPrefix+self.chatCommands[key][0]
        return -1

    def getCommandExplanation(self, command):
        for key in self.chatCommands:
            if(key == command):
                return self.chatCommands[key][1]
        return -1

    def canClientUseCommand(self, clientUID, command):
        requiredLevel = self.chatCommands[command][2]
        currentLevel = self.privGetClientLevel(clientUID)
        #self.debug("required: '{}' | current: '{}'".format(requiredLevel, currentLevel))
        if(currentLevel < requiredLevel):
            return False
        return True

    def getCommandFromAlias(self, alias):
        for commandKey in self.chatCommandAliases:
            if(alias in self.chatCommandAliases[commandKey]):
                return commandKey

        return -1

    def sendTextMessage(self, schid, targetMode, message, targetID):
        if(targetMode == ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT):
            ts3lib.requestSendPrivateTextMsg(schid, message, targetID)
        elif(targetMode == ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL):
            ts3lib.requestSendChannelTextMsg(schid, message, targetID)


    def sendLongTextMessage(self, schid, targetMode, message, targetID, leadingSplitString=""):
        splitLines = message.split("\n")

        allParts = []
        onePart = ""
        for line in splitLines:
            if(len(onePart) + len(line) >= 950):
                if(onePart != ""):
                    allParts.append(onePart)
                    onePart = ""
            if(onePart == ""):
                onePart = line
            else:
                onePart += "\n"+line
        allParts.append(onePart)

        allParts2 = []

        for part in allParts:
            if(len(part) < 950):
                allParts2.append(part)
            else:
                splitSpace = part.split(" ")
                oneSpace = ""
                self.debug("splitSpace: {}".format(len(splitSpace)))
                for space in splitSpace:
                    if(len(oneSpace) + len(space) >= 950):
                        if(oneSpace != ""):
                            allParts2.append(oneSpace)
                            oneSpace = ""
                    if(oneSpace == ""):
                        oneSpace = space
                    else:
                        oneSpace += " "+space
                allParts2.append(oneSpace)

        for i in range(0, len(allParts2)):
            if(i < len(allParts2)-1):
                part = allParts2[i]
                if(part.lower().count("[b]") == part.lower().count("[/b]")+1):
                    allParts2[i+1] = "[B]"+allParts2[i+1]
                if(part.lower().count("[u]") == part.lower().count("[/u]")+1):
                    allParts2[i+1] = "[U]"+allParts2[i+1]
                if(part.lower().count("[i]") == part.lower().count("[/i]")+1):
                    allParts2[i+1] = "[I]"+allParts2[i+1]

        #self.debug("4 {}\n\n{}\n\n{}".format(allParts[0], allParts2[1], len(allParts2)))
        i = 0
        for part in allParts2:
            if(i > 0):
                part = leadingSplitString + part
            self.sendTextMessage(schid, targetMode, part, targetID)
            i += 1


#------------------CHAT COMMANDS----------------------------


    def cmdHelp(self, schid, targetMode, targetID, senderName, parameters): # !help [command]
        if(len(parameters) == 0):

            cmdListString = ""
            count = 0
            senderUID = self.getClientUIDByName(schid, senderName)

            orderedCommands = collections.OrderedDict(sorted(self.chatCommands.items(), key = lambda x: x[0]))

            for commandKey, commandValue in orderedCommands.items():
                #self.debug("3")
                if(not self.canClientUseCommand(senderUID, commandKey)):
                    #self.debug("3.1")
                    canAccessCommand = False
                else:
                    #self.debug("3.2")
                    canAccessCommand = True

                if(canAccessCommand == False):
                    if(count == 0):
                        cmdListString = "[color=#bb0000]{}[/color]".format(self.getCommandUsage(commandKey))
                    else:
                        cmdListString += "\n[color=#bb0000]{}[/color]".format(self.getCommandUsage(commandKey))
                else:
                    if(count == 0):
                        cmdListString = "{}".format(self.getCommandUsage(commandKey))
                    else:
                        cmdListString += "\n{}".format(self.getCommandUsage(commandKey))
                count += 1


            self.sendLongTextMessage(schid, targetMode, "[B]@{}[/B]: Here is a list of all commands({}):\n\n{}\n\nType \"{}help [command]\" for more help".format(senderName, count, cmdListString, self.chatCommandPrefix), targetID, "\n")

        elif(len(parameters) == 1):
            command = parameters[0]

            if(self.getCommandExplanation(command) == -1):
                command = self.getCommandFromAlias(command)
                if(command == -1):
                    self.sendTextMessage(schid, targetMode, "[B]@{}[/B]: That command does not exist ;_;".format(senderName), targetID)
                else:
                    self.sendTextMessage(schid, targetMode, "[B]@{}[/B]: \nCommand: {}\nUsage: {}\nExplanation: {}".format(senderName, command, self.getCommandUsage(command), self.getCommandExplanation(command)), targetID)
            else:
                self.sendTextMessage(schid, targetMode, "[B]@{}[/B]: \nCommand: {}\nUsage: {}\nExplanation: {}".format(senderName, command, self.getCommandUsage(command), self.getCommandExplanation(command)), targetID)

        else:
            self.sendTextMessage(schid, targetMode, "You don't even know how to use the {}help command?! Retard... (Usage: {}help [command])".format(self.chatCommandPrefix, self.chatCommandPrefix), targetID)


    def cmdPrivilege(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) > 2 and len(parameters) < 1):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("privilege"), targetID)
            return
        targetNamePart = parameters[0]
        targetName = self.getClientNameFromSubName(schid, targetNamePart)

        if(targetName == -1):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]No username found containing '[B]{}[/B]'.[/color]".format(targetNamePart), targetID)
            return
        elif(type(targetName) == type([])):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B].[/color]".format("'[/B], [B]'".join(targetName)), targetID)
            return

        targetUID = self.getClientUIDByName(schid, targetName)



        if(len(parameters) == 1):
            level = self.privGetClientLevel(targetUID)

            if(level == 0):
                levelString = "user"
            elif(level == 1):
                levelString = "mod"
            elif(level == 2):
                levelString = "admin"
            elif(level == 3):
                levelString = "me"

            self.sendTextMessage(schid, targetMode, "[B]'{}'[/B] has the priv level [B]'{}'[/B]".format(targetName, levelString), targetID)
            return


        try:
            level = int(parameters[1])
            if(level < 0 or level > 2):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The level has to be a number from 0 to 2 or one of (user, mod, admin)[/color]", targetID)
                return
        except:
            if(parameters[1].lower() == "user" or parameters[1].lower() == "remove"):
                level = 0
            elif(parameters[1].lower() == "mod" or parameters[1].lower() == "moderator"):
                level = self.privModLevel
            elif(parameters[1].lower() == "admin"):
                level = self.privAdminLevel
            else:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The level has to be a number from 0 to 2 or one of (user, mod, admin)[/color]", targetID)
                return

        if(level == 0):
            levelString = "user"
        elif(level == 1):
            levelString = "mod"
        elif(level == 2):
            levelString = "admin"
        elif(level == 3):
            levelString = "me"
        else:
            levelString = "wut"

        if(level == 0):
            err = self.privRemoveClient(targetUID)
            if(err == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] doesn't have any special rights...[/color]".format(targetName), targetID)
                return
        else:
            err = self.privSetClientLevel(targetUID, level)
            if(err == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] already has the priv level [B]'{}'[/B][/color]".format(targetName, levelString), targetID)
                return

        self.sendTextMessage(schid, targetMode, "[color=#00aa00]Successfully set client level for [B]'{}'[/B] to [B]'{}'[/B][/color]".format(targetName, levelString), targetID)



    def cmdCoinflip(self, schid, targetMode, targetID, senderName, parameters): # !coinflip [amount]
        if(len(parameters) == 0):
            num = randint(0, 1)
            if(num == 0):
                selection = "Heads"
            else:
                selection = "Tails"


            #listMessages = [["[B]\"{}\"[/B] flipped a coin...".format(senderName), 0.5],
            #                ["...it shows [B]{}[/B]".format(selection), 2.5]]
            #self.timedCall(self.textShot, [schid, targetMode, targetID, senderName], [listMessages, 0], times=[0, listMessages[0][1], 0, 0, 0, 0], timerStorage=self.textTimers)
            self.sendTextMessage(schid, targetMode, "[B]\"{}\"[/B] flipped a coin... it shows [B]{}[/B]".format(senderName, selection), targetID)
        elif(len(parameters) == 1):
            try:
                castNum = int(parameters[0])
            except:
                self.sendTextMessage(schid, targetMode, "[B]\"{}\"[/B] is not a number, '{}' you Baka VoHiYo".format(parameters[0], senderName), targetID)
                return

            if(castNum >= 5000):
                self.sendTextMessage(schid, targetMode, "You don't even have [B]{}[/B] coins, {}...".format(castNum, senderName), targetID)
                return

            countHeads = 0
            countTails = 0
            for i in range(0, castNum):
                if(randint(0, 1) == 0):
                    countHeads += 1
                else:
                    countTails += 1

            self.sendTextMessage(schid, targetMode, "[B]\"{}\"[/B] flipped a coin [B]{}[/B] times. The coin showed [B]Heads {} time(s)[/B] and [B]Tails {} time(s)[/B].".format(senderName, castNum, countHeads, countTails), targetID)

        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("coinflip"), targetID)


    def cmdRoll(self, schid, targetMode, targetID, senderName, parameters): # !roll [from] [to]
        if(len(parameters) == 0):
            fromNum = 1
            toNum = 100
        elif(len(parameters) == 2):
            try:
                fromNum = int(parameters[0])
                toNum = int(parameters[1])
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Either one or both parameters are invalid numbers[/color]".format(self.getCommandUsage("coinflip")), targetID)
                return
        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("roll"), targetID)
            return

        num = randint(0, (toNum - fromNum)) + fromNum
        self.sendTextMessage(schid, targetMode, "[B]\"{}\"[/B] rolled a [B]{}[/B].".format(senderName, num), targetID)


    def cmdN(self, schid, targetMode, targetID, senderName, parameters):    # !n
        self.sendTextMessage(schid, targetMode, "Chu say? HYPERBRUH", targetID)


    def cmd8ball(self, schid, targetMode, targetID, senderName, parameters):    # !8ball <question>
        if(len(parameters) == 0):
            self.sendTextMessage(schid, targetMode, "[B][color=orange]Magic 8Ball[/color][/B]: You have to ask a question, my son.", targetID)
        else:
            question = ""
            for s in parameters:
                question += s

            if(question in self.magic8BallSavedAnswers):
                answer = self.magic8BallSavedAnswers[question]
            else:
                num = randint(0, len(self.magic8BallAnswers)-1)
                answer = self.magic8BallAnswers[num]
                self.magic8BallSavedAnswers[question] = answer

            self.statsIncrement("All", "8Ball questions answered", 1, 4)
            self.sendTextMessage(schid, targetMode, "[B][color=orange]Magic 8Ball[/color][/B]: \"{}\"".format(answer), targetID)


    def cmdRussianroulette(self, schid, targetMode, targetID, senderName, parameters):  # !russianroulette
        num = randint(0, 600)
        self.sendTextMessage(schid, targetMode, "[B]{}[/B] pulled the trigger...".format(senderName), targetID)
        if(num <= 100):
            if(num == 88):
                ts3lib.banclient(schid, self.getClientIDByName(schid, senderName), 3600, "[color=#ff0000]You killed yorself REAL hard.[/color]")
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]and shot himself SO HARD that his brains plattered all over the place killing half the town. Thus he is banned for 1 hour :<[/color]", targetID)
                return

            self.statsIncrement("All", "Russian roulettes lost :( ", 1, 7)
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]He died PepeHands ({})[/color]".format(num), targetID)
            ts3lib.requestClientKickFromServer(schid, self.getClientIDByName(schid, senderName), "You killed yourself.")
        else:
            self.statsIncrement("All", "Russian roulettes survived", 1, 6)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]He survived \\(^-^)/ ({})[/color]".format(num), targetID)

    def cmdSuicide(self, schid, targetMode, targetID, senderName, parameters):  # !suicide
        self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]{}[/B] killed himself...[/color]".format(senderName), targetID)
        ts3lib.requestClientKickFromServer(schid, self.getClientIDByName(schid, senderName), "You killed yourself.")



    def cmdAfk(self, schid, targetMode, targetID, senderName, parameters):
        if(self.afkEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]AFK is currently disabled.[/color]", targetID)
            return

        afk = self.amAfk(schid)
        if(len(parameters) == 0):
            if(afk):
                toggleAfk = False
            else:
                toggleAfk = True
            message = self.afkMessage
        else:
            toggleAfk = True
            message = " ".join(parameters)
        #self.debug("2, message: '{}', toggleAfk: '{}', afk: '{}'".format(message, toggleAfk, afk))

        if(toggleAfk == True and not afk):
            ts3lib.setClientSelfVariableAsString(schid, ts3defines.ClientProperties.CLIENT_NICKNAME, "{} {}".format(self.afkPrefix, self.getMyName(schid)))
            ts3lib.flushClientSelfUpdates(schid)
            printMessage = "You are now marked as [b]afk[/b] ({})".format("Message: [b]'{}'[/b]".format(message) if message != "" else "No message")

        elif(toggleAfk == True and afk):
            printMessage = "Updated the afk message to [b]'{}'[/b]".format(message)

        elif(toggleAfk == False and not afk):
            self.debug("It actually happened (1)")
            printMessage = "What the fuck?!"

        elif(toggleAfk == False and afk):
            myName = self.getMyName(schid)
            #self.debug("1, myName: '{}', startswith: '{}', endswith: '{}'".format(myName, myName.startswith(self.afkPrefix), myName.endswith(self.afkPrefix)))

            if(myName.startswith(self.afkPrefix)):
                myName = myName[len(self.afkPrefix)+1:]
            elif(myName.endswith(self.afkPrefix)):
                myName = myName[:len(myName)-len(self.afkPrefix)]
            else:
                self.debug("It actually happened (2)")

            ts3lib.setClientSelfVariableAsString(schid, ts3defines.ClientProperties.CLIENT_NICKNAME, "{}".format(myName))
            ts3lib.flushClientSelfUpdates(schid)

            printMessage = "You are no longer [b]afk[/b]"

        self.afkMessage = message
        self.sendTextMessage(schid, targetMode, "[color=#00aa00]{}[/color]".format(printMessage), targetID)


    def cmdNowplaying(self, schid, targetMode, targetID, senderName, parameters):
        if(self.npEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]NowPlaying is currently disabled.[/color]", targetID)
            return

        myUID = self.getClientUIDByID(schid, self.getMyID(schid))

        if(self.hasCooldown(myUID, "cmd:nowplaying")):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]NowPlaying is on cooldown (1 seconds).[/color]", targetID)
            return

        if(len(parameters) == 1):
            if(parameters[0] != "pp" and parameters[0] != "stats" and parameters[0] != "detailed"):
                self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("nowplaying"), targetID)
                return
            fileName = "ts_{}".format(parameters[0])
        else:
            fileName = "ts_np"

        self.setCooldown(myUID, "cmd:nowplaying", "1s")

        filePath = "{}\\{}.txt".format(self.npSCFilePath, fileName)



        file = self.getFile(filePath, "r")

        if(file == None or file == False):
            self.debug("File not found for path {}\\{}.txt".format(self.npSCFilePath, fileName))
            return

        line = file.readline()
        file.close()

        line = line.replace("{nl}", "\n")

        if(line.find("{totaltime}") != -1 and line.find("{/totaltime}") != -1):
            totalTimeMilliseconds = int(line[line.find("{totaltime}")+11:line.find("{/totaltime}")])
            line = line[:line.find("{totaltime}")+11] + line[line.find("{/totaltime}"):]
            totalTimeSeconds = int(totalTimeMilliseconds/1000)
            timeFormatted = self.formatSecondsToTimeString(totalTimeSeconds)
            line = line.replace("{totaltime}", timeFormatted)
            line = line.replace("{/totaltime}", "")

        if(line == ""):
            self.sendTextMessage(schid, targetMode, "I'm currently not playing anything.", targetID)
        else:
            self.sendTextMessage(schid, targetMode, line, targetID)



    def cmdBet(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.betEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Betting is currently disabled![/color]", targetID)
            return

        if(self.ecoIsExcluded(senderUID) or self.ecoIsBlacklisted(senderUID)):
            if(self.ecoIsExcluded(senderUID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Excluded accounts can't use bet! Type '{}excludeme' to un-exclude yourself[/color]".format(self.chatCommandPrefix), targetID)
            else:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Blacklisted accounts can't use bet![/color]", targetID)
            return

        if(len(parameters) == 2):
            if(targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry, requests have to be sent in a channel[/color]", targetID)
                return

            targetNamePart, winAmount = parameters[0], parameters[1]

            targetName = self.getClientNameFromSubName(schid, targetNamePart)

            if(targetName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No username found containing '[B]{}[/B]'.[/color]".format(targetNamePart), targetID)
                return
            elif(type(targetName) == type([])):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B].[/color]".format("'[/B], [B]'".join(targetName)), targetID)
                return

            if(targetName == senderName):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You can't challenge yourself, but you might actually be mentally challenged.[/color]".format(self.betMinAmount), targetID)
                return

            targetUID = self.getClientUIDByName(schid, targetName)

            try:
                winAmountNum = int(winAmount)
                if(winAmountNum < self.betMinAmount):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]The bet amount has to be at least [B]{} Pt(s)[/B].[/color]".format(self.betMinAmount), targetID)
                    return
                elif(winAmountNum > self.betMaxAmount):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]The bet amount has to be a maximum of [B]{} Pt(s)[/B].[/color]".format(self.betMaxAmount), targetID)
                    return
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]{}[/B] is not a number.[/color]".format(winAmount), targetID)
                return

            if(self.ecoCanPay(senderUID, winAmountNum) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You don't have enough money for this bet![/color]".format(senderName), targetID)
                return

            if(self.ecoCanPay(targetUID, winAmountNum) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]{}[/B] doesn't have enough money for this bet![/color]".format(targetName), targetID)
                return

            self.betStartOpenBet([schid, targetMode, targetID, senderName], senderUID, targetUID, winAmountNum)

            self.sendTextMessage(schid, targetMode, "[B]'{}'[/B] requests a bet against [B]{}[/B], write [B]{}b[/B] to accept".format(senderName, targetName, self.chatCommandPrefix), targetID)

            if(ts3lib.getChannelOfClient(schid, self.getClientIDByUID(schid, targetUID))[1] != self.getMyChannelID(schid)):
                self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, "[B]'{}'[/B] requests a bet against you, write [B]{}b[/B] to accept".format(senderName, self.chatCommandPrefix), self.getClientIDByUID(schid, targetUID))


        elif(len(parameters) == 0):
            if(targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry, requests can only be accepted in my channel[/color]", targetID)
                return
            if(not self.betHasBetRequest(senderUID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You don't have a bet request, [B]'{}'[/B].[/color]".format(senderName), targetID)
                return

            senderUID = self.getClientUIDByName(schid, senderName)
            winAmount = self.betGetOpenBetWinAmount(senderUID)

            if(winAmount == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Some weird ass error happened here...[/color]", targetID)
                return

            if(self.ecoCanPay(senderUID, self.betGetOpenBetWinAmount(senderUID)) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You don't have enough money for this bet, [B]{}[/B]![/color]".format(senderName), targetID)
                self.betStopBet(senderUID, False)
                return

            self.betStartOngoingBet([schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL, self.getMyChannelID(schid), senderName],senderUID)
            self.sendTextMessage(schid, targetMode, "Bet started, winning number selected! Write [B]'{}b <number>'[/B] to lock in your picks!".format(self.chatCommandPrefix), targetID)

        elif(len(parameters) == 1):
            if(parameters[0] == "cancel"):
                if(self.betHasBetRequest(senderUID)):
                    self.betStopBet(senderUID, False)
                    self.sendTextMessage(schid, targetMode, "Canceled the request you recieved.", targetID)
                elif(self.betHasRequestedBet(senderUID)):
                    self.betStopBet(senderUID, True)
                    self.sendTextMessage(schid, targetMode, "Canceled the request you sent.", targetID)
                else:
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]You haven't send or recieved a bet request, [B]'{}[/B].[/color]".format(senderName), targetID)
                return

            if(self.betHasOngoingBet(senderUID) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You don't have an ongoing bet, [B]'{}[/B].[/color]".format(senderName), targetID)
                return

            try:
                number = int(parameters[0])
                if(number > 100 or number < 1):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}[/B] is not between [B]1[/B] and [B]100[/B].[/color]".format(number), targetID)
                    return
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}[/B] is not a number.[/color]".format(parameters[0]), targetID)
                return

            err = self.betEnterNumber(senderUID, number)
            if(err == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You already locked a number in, [B]{}[/B].[/color]".format(senderName), targetID)
                return
            elif(err != 1):
                self.sendTextMessage(schid, targetMode, "[color=#00aa00]Number locked in![/color]".format(senderName), targetID)
                return


    def cmdLink(self, schid, targetMode, targetID, senderName, parameters):

        senderUID = self.getClientUIDByName(schid, senderName)

        if(len(parameters) == 1 or len(parameters) == 0):
            if(len(parameters) == 1):
                if(parameters[0] == "count"):
                    url = "http://www.vi-home.de/api_magic_link.php?action=count"
                    action = "link_count"
                else:
                    if(len(parameters[0]) != 8):
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]That is not a longID '{}'[/color]".format(parameters[0]), targetID)
                        return
                    try:
                        castLongID = int(parameters[0])
                        url = "http://www.vi-home.de/api_magic_link.php?action=get&l={}".format(castLongID)
                        action = "link_request"
                    except:
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]This is not a longID '{}'![/color]".format(parameters[0]), targetID)
                        return

            if(len(parameters) == 0):
                url = "http://www.vi-home.de/api_magic_link.php?action=get"
                action = "link_request"

            if(self.hasCooldown(senderUID, "cmd:link_request")):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Please wait a little between requesting links ([b]{}[/b] cooldown)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:link_request")), targetID)
                return

            self.nwmc = QNetworkAccessManager()
            self.nwmc.connect("finished(QNetworkReply*)", self.linkReply)
            self.linkParams = {"schid": schid, "targetMode": targetMode, "targetID": targetID, "senderName": senderName, "action" : action}
            self.nwmc.get(QNetworkRequest(QUrl(url)))

            self.setCooldown(senderUID, "cmd:link_request", "10s")
            return


        senderID = self.getClientIDByName(schid, senderName)
        myUID = self.getClientUIDByID(schid, self.getMyID(schid))

        if((parameters[0] == "add" or parameters[0] == "remove") and self.privGetClientLevel(senderUID) < self.privModLevel):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], only moderators and higher can add and remove links.[/color]".format(senderName), targetID)
            return

        if(parameters[0] == "add"):
            if(self.hasCooldown(myUID, "cmd:link_add") and not self.isMe(schid, senderID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That command is currently on cooldown ([b]{}[/b] cooldown)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:link_add")), targetID)
                return

            sendUrl = parameters[1]

            if(len(parameters) > 2):
                title = " ".join(parameters[2:])
            else:
                title = ""

            if(not sendUrl.startswith("[URL]") or not sendUrl.endswith("[/URL]")):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That is not a valid url [B]'{}'[/B][/color]".format(sendUrl), targetID)
                return
            if(":" in title):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The title cannot contain any colons: '{}'[/color]".format(title.replace(":", "[B]:[/B]")), targetID)
                return

            sendUrl = sendUrl[5:-6]

            if(title == ""):
                url = "http://www.vi-home.de/api_magic_link.php?action=add&k={}&url={}".format(self.apiKey, sendUrl)
            else:
                url = "http://www.vi-home.de/api_magic_link.php?action=add&k={}&url={}&message={}".format(self.apiKey, sendUrl, title)

            action = "link_add"

            if(not self.isMe(schid, senderID)):
                self.setCooldown(myUID, "cmd:link_add", "3m")

        elif(parameters[0] == "remove" and len(parameters) == 2):
            if(self.hasCooldown(myUID, "cmd:link_remove") and not self.isMe(schid, senderID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That command is currently on cooldown ([b]{}[/b] cooldown)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:link_remove")), targetID)
                return

            sendUrl = parameters[1]

            useLongID = False

            if(not sendUrl.startswith("[URL]") or not sendUrl.endswith("[/URL]")):
                if(len(sendUrl) == 8):
                    try:
                        longID = int(sendUrl)
                        useLongID = True
                    except:
                        pass
                if(useLongID == False):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]That is not a valid url or longID [B]'{}'[/B][/color]".format(sendUrl), targetID)
                    return

            if(useLongID == False):
                sendUrl = sendUrl[5:-6]
                url = "http://www.vi-home.de/api_magic_link.php?action=remove&k={}&url={}".format(self.apiKey, sendUrl)
            else:
                url = "http://www.vi-home.de/api_magic_link.php?action=remove&k={}&l={}".format(self.apiKey, longID)

            action = "link_remove"

            if(not self.isMe(schid, senderID)):
                self.setCooldown(senderUID, "cmd:link_remove", "3m")

        self.nwmc = QNetworkAccessManager()
        self.nwmc.connect("finished(QNetworkReply*)", self.linkReply)
        self.linkParams = {"schid": schid, "targetMode": targetMode, "targetID": targetID, "senderName": senderName, "action" : action}
        self.nwmc.get(QNetworkRequest(QUrl(url)))


    def cmdHighlight(self, schid, targetMode, targetID, senderName, parameters):    # !highlight <list|add|remove> [keyword]
        if(self.highlightEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Highlight is currently disabled.[/color]", targetID)
            return

        if(len(parameters) == 1 and parameters[0] == "list"):
            file = self.getFile(self.highlightFileName, "r")

            strAllKeywords = ""

            count = 0
            for keyword in file:
                keyword = keyword.rstrip()

                if(count == 0):
                    strAllKeywords = "{}: ".format(count+1)+keyword
                else:
                    strAllKeywords += "\n{}: ".format(count+1)+keyword

                count += 1

            if(count == 0):
                self.sendTextMessage(schid, targetMode, "There are no highlighted keywords...", targetID)
            else:
                self.sendTextMessage(schid, targetMode, "Here are all highlighted keywords:\n\n{}".format(strAllKeywords), targetID)

            file.close()
        else:
            if(parameters[0] == "add"):
                for keyword in parameters:
                    if(keyword != "add"):
                        if(self.addHighlightKeyword(keyword) == -1):
                            self.sendTextMessage(schid, targetMode, "[color=#cc0000]Keyword '{}' already exists, skipping...[/color]".format(keyword), targetID)
                        else:
                            self.sendTextMessage(schid, targetMode, "[color=#00aa00]Keyword '[B]{}[/B]' successfully added.[/color]".format(keyword), targetID)

            elif(parameters[0] == "remove"):
                for keyword in parameters:
                    if(keyword != "remove"):
                        if(self.removeHighlightKeyword(keyword) == -1):
                            self.sendTextMessage(schid, targetMode, "[color=#cc0000]Keyword '{}' doesn't exist, skipping...[/color]".format(keyword), targetID)
                        else:
                            self.sendTextMessage(schid, targetMode, "[color=#00aa00]Keyword '[B]{}[/B]' successfully removed.[/color]".format(keyword), targetID)

            else:
                self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("highlight"), targetID)

    def cmdExcludeme(self, schid, targetMode, targetID, senderName, parameters):  # !excludeme [blacklist|exclude] [<name>|<clientUID>]
        if(len(parameters) != 0):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("excludeme"), targetID)
            return

        senderUID = self.getClientUIDByName(schid, senderName)
        if(self.hasCooldown(senderUID, "cmd:excludeme")):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]This command is still on cooldown, [B]'{}'[/B]. ([b]{}[/b] cooldown)[/color]".format(senderName, self.getRemainingCooldownAsString(senderUID, "cmd:excludeme")), targetID)
            return

        if(self.ecoIsExcluded(senderUID)):
            self.ecoRemoveExcluded(senderUID)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]You are no longer excluded, [B]'{}'[/B].[/color]".format(senderName), targetID)
            self.setCooldown(senderUID, "cmd:excludeme", "60m")

        elif(self.ecoIsBlacklisted(senderUID)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]You are blacklisted from using the economy system and can't un-exclude yourself, [B]'{}'[/B].[/color]".format(senderName), targetID)

        elif(self.ecoIsWhitelisted(senderUID)):
            self.ecoRemoveExcluded(senderUID)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]Instead of being whitelisted, you are now being excluded, [B]'{}'[/B].[/color]".format(senderName), targetID)
            self.setCooldown(senderUID, "cmd:excludeme", "60m")

        else:
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]You are now being excluded, [B]'{}'[/B].[/color]".format(senderName), targetID)
            self.ecoSetExclusionValue(senderUID, self.ecoExcluded)
            self.setCooldown(senderUID, "cmd:excludeme", "60m")



    def cmdBlacklist(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)

        if(len(parameters[0]) == 28):
            targetUID = parameters[0]
            targetName = targetUID
        else:
            targetName = self.getClientNameFromSubName(schid, " ".join(parameters[0:]))

            if(targetName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No username found containing '[B]{}[/B]'.[/color]".format(parameters[0]), targetID)
                return
            elif(type(targetName) == type([])):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B].[/color]".format("'[/B], [B]'".join(targetName)), targetID)
                return

            targetUID = self.getClientUIDByName(schid, targetName)


        if(self.ecoIsBlacklisted(targetUID)):
            self.ecoRemoveExcluded(targetUID)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer blacklisted.[/color]".format(targetName), targetID)

        elif(self.ecoIsWhitelisted(targetUID)):
            self.ecoSetExclusionValue(targetUID, self.ecoBlacklisted)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer whitelisted and is now blacklisted.[/color]".format(targetName), targetID)

        else:
            self.ecoSetExclusionValue(targetUID, self.ecoBlacklisted)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is now blacklisted.[/color]".format(targetName), targetID)


    def cmdExclude(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)

        if(len(parameters[0]) == 28):
            targetUID = parameters[0]
            targetName = targetUID
        else:
            targetName = self.getClientNameFromSubName(schid, " ".join(parameters[0:]))

            if(targetName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No username found containing '[B]{}[/B]'.[/color]".format(parameters[0]), targetID)
                return
            elif(type(targetName) == type([])):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B].[/color]".format("'[/B], [B]'".join(targetName)), targetID)
                return

            targetUID = self.getClientUIDByName(schid, targetName)

        if(self.ecoIsBlacklisted(targetUID)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] is blacklisted and can't be un-excluded.[/color]".format(targetName), targetID)

        elif(self.ecoIsExcluded(targetUID)):
            self.ecoRemoveExcluded(targetUID)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer excluded.[/color]".format(targetName), targetID)

        elif(self.ecoIsWhitelisted(targetUID)):
            self.ecoSetExclusionValue(targetUID, self.ecoExcluded)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer whitelisted and is now excluded.[/color]".format(targetName), targetID)

        else:
            self.ecoSetExclusionValue(targetUID, self.ecoExcluded)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is now excluded.[/color]".format(targetName), targetID)


    def cmdWhitelist(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)

        if(len(parameters[0]) == 28):
            targetUID = parameters[0]
            targetName = targetUID
        else:
            targetName = self.getClientNameFromSubName(schid, " ".join(parameters[0:]))

            if(targetName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No username found containing '[B]{}[/B]'.[/color]".format(parameters[0]), targetID)
                return
            elif(type(targetName) == type([])):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B].[/color]".format("'[/B], [B]'".join(targetName)), targetID)
                return

            targetUID = self.getClientUIDByName(schid, targetName)


        if(self.ecoIsBlacklisted(targetUID)):
            self.ecoSetExclusionValue(targetUID, self.ecoWhitelisted)
            self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] is no longer blacklisted and is now whitelisted.[/color]".format(targetName), targetID)

        elif(self.ecoIsExcluded(targetUID)):
            self.ecoSetExclusionValue(targetUID, self.ecoWhitelisted)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer excluded and is now whitelisted.[/color]".format(targetName), targetID)

        elif(self.ecoIsWhitelisted(targetUID)):
            self.ecoRemoveExcluded(targetUID)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is no longer whitelisted.[/color]".format(targetName), targetID)

        else:
            self.ecoSetExclusionValue(targetUID, self.ecoWhitelisted)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] is now whitelisted.[/color]".format(targetName), targetID)




    def cmdBalance(self, schid, targetMode, targetID, senderName, parameters):  # !balance [send] [<name>] [<amount>]
        if(len(parameters) == 0):

            clientUID = self.getClientUIDByName(schid, senderName)
            if(self.ecoIsBlacklisted(clientUID) == True):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], but your account is frozen.[/color]".format(senderName), targetID)
                return
            balance = self.ecoGetBalance(clientUID)

            #if(self.ecoIsExcluded(clientUID)):
                #self.sendTextMessage(schid, targetMode, "[B]@{}[/B]: Your balance: {} Pts\n[color=#ff0000]!!Warning: Your account is excluded, [B]'{}'[/B]. You can't use tokens and don't recieve points over time!![/color]".format(senderName, balance, senderName), targetID)
            #else:
            self.sendTextMessage(schid, targetMode, "[B]@{}[/B]: Your balance: {} Pts".format(senderName, balance), targetID)
            return


        elif(len(parameters) == 3):
            if(parameters[0] != "send"):
                self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("balance"), targetID)
                return

            senderUID = self.getClientUIDByName(schid, senderName)

            if(self.ecoIsBlacklisted(senderUID) or (self.ecoIsExcluded(senderUID) and False)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], but you can't send money while your account is frozen![/color]".format(senderName), targetID)
                return

            if(self.ecoIsExcluded(senderUID)):
                if(self.hasCooldown(senderUID, "cmd:send_balance")):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Excluded accounts can only send balance once every 5 minutes. ([B]{}[/B] cooldown left)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:send_balance")), targetID)
                    return

            try:
                sendAmount = int(parameters[2])
            except:
                if(parameters[2] == "all"):
                    sendAmount = self.ecoGetBalance(senderUID)
                elif(parameters[2] == "excess"):
                    if(self.ecoGetBalance(senderUID) > self.ecoPointLimit):
                        sendAmount = self.ecoGetBalance(senderUID) - self.ecoPointLimit
                    else:
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]@'{}'[/B]: You don't have any excess balance! (Your balance: [b]{}[/b])[/color]".format(senderName, self.ecoGetBalance(senderUID)), targetID)
                        return
                else:
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] is neither a whole number nor 'all'/'excess'[/color]".format(parameters[2]), targetID)
                    return


            if(self.ecoIsExcluded(senderUID)):
                if(sendAmount > self.ecoExcludedMaxSendAmount):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]Excluded accounts can only send [b]'{}'[/b] balance at once.[/color]".format(self.ecoExcludedMaxSendAmount), targetID)
                    return



            recieverName = self.getClientNameFromSubName(schid, parameters[1])

            if(recieverName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No client with [B]'{}'[/B] in their name was found[/color]".format(parameters[1]), targetID)
                return

            elif(type(recieverName) == type(["test"])):
                sendString = ""
                for string in recieverName:
                    sendString += "\n{}".format(string)
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple people found with [B]'{}'[/B] in their name:[B]'{}'[/B][/color]".format(parameters[1], sendString), targetID)
                return


            recieverUID = self.getClientUIDByName(schid, recieverName)

            if(sendAmount < 1):
                if(sendAmount == 0):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Why would you want to send nothing, [B]'{}'[/B]??[/color]".format(senderName), targetID)
                else:
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]You just lost 1 point for trying bullshit, [B]'{}'[/B][/color]".format(senderName), targetID)
                    if(self.ecoGetBalance(senderUID) == 0):
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]Oh wait, you don't even have 1 point, stupid n...[/color]", targetID)
                    else:
                        self.ecoChangeBalance(senderUID, -1)
                return

            if(senderUID == recieverUID):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Great [B]'{}'[/B], you are trying to send yourself money...[/color]".format(senderName), targetID)
                return

            if(not self.ecoCanPay(senderUID, sendAmount)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You don't have enough balance for that, [B]'{}'[/B][/color]".format(senderName, recieverName, sendAmount), targetID)
                return

            if(self.ecoGetBalance(recieverUID) + sendAmount > self.ecoPointLimit and self.ecoIsExcluded(recieverUID) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The target would have more than 100 balance, sorry @[B]'{}'[/B][/color]".format(senderName, recieverName, sendAmount), targetID)
                return

            if(self.ecoIsExcluded(senderUID)):
                self.setCooldown(senderUID, "cmd:send_balance", "5m")

            self.sendTextMessage(schid, targetMode, "[B]'{}'[/B] sent [B]'{}'[/B] {} Pt(s) ([color=#990000]{}-{}[/color]  ->  [color=#009900]{}+{}[/color])".format(senderName, recieverName, sendAmount, self.ecoGetBalance(senderUID), sendAmount, self.ecoGetBalance(recieverUID), sendAmount), targetID)
            self.ecoTransferBalance(senderUID, recieverUID, sendAmount)

            return



        elif(len(parameters) == 1):
            if(parameters[0] == "-serveruid"):
                (err, serverUID) = ts3lib.getServerVariable(schid, ts3defines.VirtualServerProperties.VIRTUALSERVER_UNIQUE_IDENTIFIER)
                if(err == ts3defines.ERROR_ok):
                    self.sendTextMessage(schid, targetMode, "This server's UID: [color=#0000aa]{}[/color]".format(serverUID), targetID)
                return

            targetName = self.getClientNameFromSubName(schid, parameters[0])

            if(targetName == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]No client with [B]'{}'[/B] in their name was found[/color]".format(parameters[0]), targetID)
                return

            elif(type(targetName) == type(["test"])):
                sendString = ""
                for string in targetName:
                    sendString += "\n{}".format(string)
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple people found with [B]'{}'[/B] in their name:[B]'{}'[/B][/color]".format(parameters[0], targetName), targetID)
                return

            targetUID = self.getClientUIDByName(schid, targetName)


            if(self.ecoIsBlacklisted(targetUID) == True):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The account of [B]{}[/B] is frozen and cannot be accessed.[/color]".format(targetName), targetID)
                return
            balance = self.ecoGetBalance(targetUID)

            #if(self.ecoIsExcluded(targetUID)):
            #    self.sendTextMessage(schid, targetMode, "[B]@{}[/B]'s balance: {} Pts\n[color=#ff0000]!!Warning: [B]{}[B]'s account is excluded. They can't use tokens and don't recieve points over time!![/color]".format(targetName, balance, targetName), targetID)
            #else:
            self.sendTextMessage(schid, targetMode, "[B]@{}[/B]'s balance: {} Pts".format(targetName, balance), targetID)

            return

        self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("balance"), targetID)



    def cmdRoulette(self, schid, targetMode, targetID, senderName, parameters):
        #!roulette <amount[%] or all>

        if(self.roulEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Roulette is currently disabled, sorry [B]'{}'[/B][/color]".format(senderName), targetID)
            return

        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.ecoIsExcluded(senderUID) or self.ecoIsBlacklisted(senderUID)):
            if(self.ecoIsExcluded(senderUID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Excluded accounts can't use roulette! Type '{}excludeme' to un-exclude yourself[/color]".format(self.chatCommandPrefix), targetID)
            else:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Blacklisted accounts can't use roulette![/color]", targetID)
            return

        if(parameters[0] == "stats"):
            if(len(parameters) == 1):
                targetUID = senderUID
                targetName = senderName
            else:
                partName = " ".join(parameters[1:])
                targetName = self.getClientNameFromSubName(schid, partName)

                if(targetName == -1):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]No client with [B]'{}'[/B] in their name was found[/color]".format(partName), targetID)
                    return

                elif(type(targetName) == type(["test"])):
                    sendString = ""
                    for string in targetName:
                        sendString += "\n{}".format(string)
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple people found with [B]'{}'[/B] in their name:[B]'{}'[/B][/color]".format(partName, sendString), targetID)
                    return

                targetUID = self.getClientUIDByName(schid, targetName)

            net = self.roulGetNet(targetUID)

            netColor = "#ff0000" if net < 0 else ("#00aa00" if net > 0 else "#000000")
            netString = "loss" if net < 0 else "gain"
            netPrefix = "" if net < 0 else ("+" if net > 0 else "+-")

            self.sendTextMessage(schid, targetMode, "So far, [B]@{}[/B] has made a net {} of [B][color={}]{}{}[/color] Pts[/B].".format(targetName, netString, netColor, netPrefix, net), targetID)
            return

        if(len(parameters) != 1):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("roulette"), targetID)
            return

        if(self.hasCooldown(senderUID, "cmd:roulette")):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]This command is still on cooldown. ([b]{}[/b] cooldown)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:roulette")), targetID)
            return

        if(self.ecoGetBalance(senderUID) > self.ecoPointLimit):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]You have too many points and can't gamble right now, [B]'{}'[/B][/color]".format(senderName), targetID)
            return

        amountUnparsed = parameters[0]

        if(amountUnparsed.lower() == "all"):
            gambleAmount = self.ecoGetBalance(senderUID)
            if(gambleAmount <= 0):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You have no money, now get the hell outa my casino[/color]", targetID)
                return

        elif(amountUnparsed.endswith("%")):
            try:
                percentVal = float(amountUnparsed[0:-1])
                if(percentVal == int(percentVal)):
                    percentVal = int(percentVal)
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]\"{}\" is not a valid roulette expression.[/color]".format(amountUnparsed), targetID)
                return

            if(percentVal <= 0 or percentVal > 100):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The gamble percent amount has to be more than [B]0%[/B] and not higher than [B]100.000.000%[/B].[/color]".format(amountUnparsed), targetID)
                return

            gambleAmount = self.max(int(round(self.ecoGetBalance(senderUID) * (percentVal/100), 0)), 1)
            if(self.ecoCanPay(senderUID, gambleAmount) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You can't afford to gamble [B]{}[/B] Pts., [B]'{}'[/B][/color]".format(gambleAmount, senderName), targetID)
                return

        else:
            try:
                gambleAmount = int(amountUnparsed)
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]\"{}\"[/B] is not a valid integer.[/color]".format(amountUnparsed), targetID)
                return

            if(gambleAmount <= 0):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You have no money, now get the hell outa my casino[/color]", targetID)
                return

            if(self.ecoCanPay(senderUID, gambleAmount) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You can't afford to gamble [B]'{}'[/B] Pts., [B]'{}'[/B][/color]".format(gambleAmount, senderName), targetID)
                return

        num = randint(1, 1001)

        if(num <= self.roulWinChancePercent * 10):
            winAmount = gambleAmount * (self.roulWinMultiplier-1)

            if(num == 420):
                self.sendTextMessage(schid, targetMode, "[color=#00aa00]\t-\t420 Blaze it Bonus! Your reward has just been tripled!\t-\t[/color]", targetID)
                self.play_wav(self.getPathToFile("playsounds")+"/"+self.roulSpecialWinSound+".wav", 1, 1)

                winAmount *= 3
                self.statsIncrement("Roulette", "420-Bonuses", 1, 99)


            self.roulChangeNet(senderUID, winAmount)
            self.ecoChangeBalance(senderUID, winAmount)
            self.statsIncrement("Roulette", "Wins", 1, 0)
            self.statsIncrement("Roulette", "Total balance won", gambleAmount, 2)
            self.statsIncrement("Roulette", "Total balance made", winAmount, 4)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00][B]'{}'[/B] won [B]{}[/B] Pts. in roulette and now has [B]{}[/B] Pts. forsenPls[/color]".format(senderName, winAmount, self.ecoGetBalance(senderUID)), targetID)

            if(winAmount == self.ecoPointLimit):
                self.play_wav(self.getPathToFile("playsounds")+"/"+self.roulCashSound+".wav", 1, 1)
        else:
            loseAmount = -gambleAmount
            self.roulChangeNet(senderUID, loseAmount)
            self.ecoChangeBalance(senderUID, loseAmount)
            self.statsIncrement("Roulette", "Loses", 1, 1)
            self.statsIncrement("Roulette", "Total balance lost", gambleAmount, 3)
            self.statsIncrement("Roulette", "Total balance made", loseAmount, 4)
            self.sendTextMessage(schid, targetMode, "[color=#bb0000][B]'{}'[/B] lost [B]{}[/B] Pts. in roulette and now has [B]{}[/B] Pts. FeelsBadMan[/color]".format(senderName, -loseAmount, self.ecoGetBalance(senderUID)), targetID)

        self.setCooldown(senderUID, "cmd:roulette", self.roulCooldown)



    def cmdYoutube(self, schid, targetMode, targetID, senderName, parameters, short=False):      # !youtube <url|watch id>
        self.cmdYoutubeAlternate(schid, targetMode, targetID, senderName, parameters, short)
        # if(len(parameters) == 1):
        #     if(parameters[0].lower().startswith("[url]") and parameters[0].lower().endswith("[/url]")):
        #         link = parameters[0][5:-6]
        #
        #         dictInfo = self.pafyLookup(link)
        #
        #         if(type(dictInfo) == type("string")):
        #             #self.sendTextMessage(schid, targetMode, "[color=#ff0000]Pafy lookup failed, trying backup method...[/color]", targetID)
        #             self.cmdYoutubeAlternate(schid, targetMode, targetID, senderName, parameters, short)
        #             return
        #         else:
        #             likes = dictInfo["Likes"]
        #             dislikes = dictInfo["Dislikes"]
        #
        #             if(dislikes == 0 and likes == 0):
        #                 ratio = "N/A"
        #                 color = "ff0000"
        #             else:
        #                 ratio = int((likes / (likes + dislikes))*100)
        #                 if(ratio >= 95):
        #                     color = "00aa00"
        #                 elif(ratio >= 85):
        #                     color = "bbbb00"
        #                 else:
        #                     color = "ff0000"
        #
        #             if(ratio == 100):
        #                 colorString = "[color=#22ee22]↑({}%)↑[/color]".format(ratio)
        #             else:
        #                 colorString = "[color=#{}]({}%)[/color]".format(color, ratio)
        #
        #
        #             self.sendTextMessage(schid, targetMode,
        #                                  "[color=#aa0000]YouTube(Pafy)[/color] ⇒ [url={}]{}[/url] [{}] from [B]{}[/B] | [color=#00aa00]↑{}[/color] | [color=#ff0000]↓{}[/color] | {}".format(link,
        #                                                                                                                                                                                  dictInfo["Title"],
        #                                                                                                                                                                                  dictInfo["Duration"],
        #                                                                                                                                                                                  dictInfo["Author"],
        #                                                                                                                                                                                  dictInfo["Likes"],
        #                                                                                                                                                                                  dictInfo["Dislikes"],
        #                                                                                                                                                                                  colorString,
        #                                                                                                                                                                                  ratio), targetID)
        #
        #     else:
        #         self.sendTextMessage(schid, targetMode, "[color=#ff0000]The given parameter is not a url.[/color]", targetID)

    def cmdYoutubeAlternate(self, schid, targetMode, targetID, senderName, parameters, short=False):
        if(len(parameters) == 1):
            #self.debug("1 '{}' 2 '{}' 3 '{}'".format(parameters[0].lower().startswith("[url]"), parameters[0].lower().endswith("[/url]"), parameters[0].lower().startswith("http")))
            #self.debug("parameters: '{}'".format(parameters))

            if(parameters[0].lower().startswith("[url]") and parameters[0].lower().endswith("[/url]")):
                link = parameters[0][5:-6]

                if(link.startswith("http") and "youtube" in link and "watch?v" in link):
                    url = link
                elif(link.startswith("http") and "youtu.be" in link):
                    if("?" in link):
                        url = "https://www.youtube.com/watch?v={}".format(link[self.findnth(link, "/", 2)+1:link.find("?")])
                        time = link[link.find("?")+3:]
                    else:
                        url = "https://www.youtube.com/watch?v={}".format(link[self.findnth(link, "/", 2)+1:])
                else:
                    self.sendTextMessage(schid, targetMode, "That is no YouTube URL...", targetID)
                    return
            else:
               url = "https://www.youtube.com/watch?v={}".format(parameters[0])

            self.ytResponseTarget.append(schid)
            self.ytResponseTarget.append(targetMode)
            self.ytResponseTarget.append(targetID)
            self.ytResponseTarget.append(senderName)
            self.ytResponseTarget.append(url)
            self.ytResponseTarget.append(short)
            if("time" in locals()):
                self.ytResponseTarget.append(time)

            self.nwmc = QNetworkAccessManager()
            self.nwmc.connect("finished(QNetworkReply*)", self.ytNetworkReply)
            self.nwmc.get(QNetworkRequest(QUrl(url)))

        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("youtube"), targetID)


    def cmdTwitchclip(self, schid, targetMode, targetID, senderName, parameters, short=False):      # !twitchclip <url>
        if(len(parameters) == 1):
            #self.debug("1 '{}' 2 '{}' 3 '{}'".format(parameters[0].lower().startswith("[url]"), parameters[0].lower().endswith("[/url]"), parameters[0].lower().startswith("http")))

            if(parameters[0].lower().startswith("[url]") and parameters[0].lower().endswith("[/url]")):
                link = parameters[0][5:-6]

                if(link.startswith("http") and "clips.twitch.tv" in link):
                    url = link
                else:
                    self.sendTextMessage(schid, targetMode, "That is no Twitch Clip URL...", targetID)
                    return

            self.twitchResponseTarget.append(schid)
            self.twitchResponseTarget.append(targetMode)
            self.twitchResponseTarget.append(targetID)
            self.twitchResponseTarget.append(senderName)
            self.twitchResponseTarget.append(url)

            self.nwmc = QNetworkAccessManager()
            self.nwmc.connect("finished(QNetworkReply*)", self.twitchNetworkReply)
            self.nwmc.get(QNetworkRequest(QUrl(url)))

        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("twitchclip"), targetID)


    def cmdUpload(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) == 0):
            self.sendTextMessage(schid, targetMode, "Upload files via: [URL]"+self.UPLOAD_URL.format(self.uploadAPIKeyPublic)+"[/URL]", targetID)
            return

        senderUID = self.getClientUIDByName(schid, senderName)
        if(self.privGetClientLevel(senderUID) != self.privMyLevel):
            self.sendTextMessage(schid, targetMode, "You can't upload files from my computer >:C", targetID)
            return

        path = parameters[0]
        password = ""
        if(len(parameters) >= 2):
            password = " ".join(parameters[2:])

        params = {"key": self.uploadAPIKeyPrivate, "result_as": "json"}
        data = {}
        if(password != ""):
            data['password'] = password

        try:
            files = {"file": open(path, 'rb')}
            r = requests.post(self.DIRECT_UPLOAD_URL, params=params, files=files, data=data)
            response = json.loads(r.text)
            if(response['errorCode'] == "OK"):
                token = response['token']
                self.previousToken = token
                self.sendTextMessage(schid, targetMode, "Download file [b]'{}'[/b] => [URL]{}[/URL]".format(response['filename'], self.DOWNLOAD_URL.format(token)), targetID)
            else:
                self.sendTextMessage(schid, targetMode, "An error occurred while trying to upload: "+response['errorMessage'], targetID)
        except Exception as e:
            self.sendTextMessage(schid, targetMode, "An error occurred while trying to upload: "+e, targetID)


    def cmdPaypal(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) != 1):
            self.sendTextMessage(schid, targetMode, "[B]@'{}'[/B]: Use this link to buy new points (1Cent = 1 Pt.): [url=https://www.paypal.me/HannesVI]https://www.paypal.me/HannesVI[/url]".format(senderName), targetID)
            return

        elif(len(parameters) == 1):
            try:
                cast = int(parameters[0])
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]'[B]{}[/B]' is not a valid whole number, '[B]{}[/B]'[/color]".format(parameters[0], senderName), targetID)
                return
            self.sendTextMessage(schid, targetMode, "[B]@'{}'[/B]: Use this link to buy new points (1Cent = 1 Pt.): [url=https://www.paypal.me/HannesVI/{}]https://www.paypal.me/HannesVI/{}[/url]".format(senderName, cast, cast), targetID)
            return


    def cmdSetvariable(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) < 2):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("setvariable"), targetID)
        else:
            var = parameters[0]
            if(not hasattr(self, var)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That variable was not found: '[B]{}[/B]'[/color]".format(var), targetID)
                return
            string = " ".join(parameters[1:])
            val = self.parseVariableFromString(string)

            if(type(getattr(self, var)) != type(val)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The entered variables type '[B]{}[/B]' doesn't match the type of '[B]{}[/B]': [B]{}[/B][/color]".format(type(val), var, type(getattr(self, var))), targetID)
                return

            setattr(self, var, val)
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]Successfully set '[B]{}[/B]' to value '[B]{}[/B]'[/color]".format(var, val), targetID)


    def cmdVarvalue(self, schid, targetMode, targetID, senderName, parameters):     #!varval chat tts
        if(len(parameters) < 1):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("varvalue"), targetID)
        else:
            findSimilar = False
            if(parameters[0] == "-all"):
                findSimilar = True

            for var in parameters:
                if(var == "-all"):
                    pass
                elif(var == "acPrivKey"):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]For security purposes variable '[B]{}[/B]' cannot be read![/color]".format(var), targetID)

                elif(hasattr(self, var)):
                    if(callable(getattr(self, var))):#Case method
                        lines = inspect.getsourcelines(getattr(self, var))
                        self.sendLongTextMessage(schid, targetMode, "[B]Source code of method '{}':[/B] \n{}".format(var, "".join(lines[0])), targetID, "\n")
                        #self.debug("".join(lines[0]))
                        return
                    val = "{}".format(getattr(self, var))
                    self.sendLongTextMessage(schid, targetMode, "Variable '[B]{}[/B]' has the value: '[B]{}[/B]'".format(var, val), targetID)
                else:
                    if(findSimilar == False):
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]Variable '[B]{}[/B]' was not found, skipping...[/color]".format(var), targetID)
                    else:
                        foundOnce = False
                        for selfVarName in dir(self):
                            if(selfVarName == "acPrivKey" and var in selfVarName):
                                self.sendTextMessage(schid, targetMode, "[color=#ff0000]For security purposes variable '[B]{}[/B]' cannot be read![/color]".format(selfVarName), targetID)

                            elif(var in selfVarName):
                                if(not callable(getattr(self, selfVarName))):
                                    foundOnce = True
                                    self.sendLongTextMessage(schid, targetMode, "Similar Variable '[B]{}[/B]' has the value: '[B]{}[/B]'".format(selfVarName.replace(var, "[color=#ff0000]{}[/color]".format(var)), getattr(self, selfVarName)), targetID)

                        if(foundOnce == False):
                            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Found no variable containing '[B]{}[/B]'[/color]".format(var), targetID)


    def cmdTest(self, schid, targetMode, targetID, senderName, parameters):
        self.debug("1")
        self.tssStarted = True
        ts3lib.requestServerGroupList(schid)
        self.debug("3")


    def cmdGoogle(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) < 1):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("google"), targetID)
        else:
            try:
                from urllib.parse import quote_plus
                googleAPI = "https://www.googleapis.com/customsearch/v1"
                googleAPIKey = "AIzaSyDj5tgIBtdiL8pdVV_tqm7aw45jjdFP1hw"
                googleSearchID = "008729515406769090877:33fok_ycoaa"
                parameters = " ".join(parameters)
                params = quote_plus(parameters)
                url = "{0}?key={1}&cx={2}&q={3}".format(googleAPI, googleAPIKey, googleSearchID, params)
                self.nwmc = QNetworkAccessManager()
                self.nwmc.connect("finished(QNetworkReply*)", self.googleReply)
                self.cmdevent = {"event": "", "returnCode": "", "schid": schid, "targetMode": targetMode, "toID": targetID, "params": params}
                self.nwmc.get(QNetworkRequest(QUrl(url)))
            except: from traceback import format_exc;ts3lib.logMessage(format_exc(), ts3defines.LogLevel.LogLevel_ERROR, "pyTSon", 0)

    def cmdWakemeup(self, schid, targetMode, targetID, senderName, parameters):
        senderID = self.getClientIDByName(schid, senderName)

        if(len(parameters) == 0):
            ts3lib.requestClientPoke(schid, senderID, "[WMU] WAKEY WAKEY RISE AND SHINE")
        else:
            message = " ".join(parameters)
            ts3lib.requestClientPoke(schid, senderID, "[WMU] {}".format(message))


    def cmdTodo(self, schid, targetMode, targetID, senderName, parameters):
        self.debug("TODO")


    def cmdCommand(self, schid, targetMode, targetID, senderName, parameters):
        self.debug("TODO")


    def cmdTimed(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) < 2):
            if(len(parameters) != 1):
                self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("timed"), targetID)
            elif(parameters[0] == "cancel"):

                for timer in self.singleShotParams:
                    listParams = self.singleShotParams[timer]
                    clientName = self.getClientNameByUID(schid, listParams[1][3])

                    if(clientName == senderName):
                        timer.stop()
                        del(self.singleShotParams[timer])
                        self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] Your timer was canceled".format(senderName), targetID)
                        return

                self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] [color=#ff0000]A running timer for you was not found...[/color]".format(senderName), targetID)
            elif(parameters[0] == "list"):
                for timer in self.singleShotParams:
                    listParams = self.singleShotParams[timer]
                    clientName = self.getClientNameByUID(schid, listParams[1][3])
                    if(clientName == senderName):
                        seconds = timer.interval / 1000

                        #ToDo: Parse seconds to all other time units

                        self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] You have a timer running with the interval [B]'{} seconds'[/B] for the command [B]'{}'[/B]".format(senderName, seconds, listParams[0].__name__[3:4].lower()+listParams[0].__name__[4:]), targetID)
                        return

                self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] [color=#ff0000]A running timer for you was not found...[/color]".format(senderName), targetID)
            else:
                self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("timed"), targetID)

        else:

            senderUID = self.getClientUIDByName(schid, senderName)

            for timer in self.singleShotParams:
                listParams = self.singleShotParams[timer]
                clientName = self.getClientNameByUID(schid, listParams[1][3])
                if(clientName == senderName):
                    self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] [color=#ff0000]You already have a running timer for the command [B]'{}'[/B][/color]\nType \"{}timed cancel\" to cancel the timer".format(parameters[1], listParams[0].__name__, self.chatCommandPrefix), targetID)
                    return


            cmd = parameters[1]
            if(self.isCommand(cmd) == False):
                cmd = self.getCommandFromAlias(cmd)
                if(cmd == -1):
                    self.sendTextMessage(schid, targetMode, "That parameter is not a valid command: \"{}\"".format(parameters[1]), targetID)
                    return

            if(cmd == "timed"):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Halt deine scheiß Schnauze du behinderter Bastard, [B]@'{}'[/B]".format(senderName), targetID)
                return

            if(not self.canClientUseCommand(senderUID, cmd)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], you can't use that command.[/color]".format(fromName), targetID)
                return

            method = getattr(self, "cmd"+cmd[0:1].upper()+cmd[1:])

            times = self.getTimesFromTimeString(parameters[0])

            if(times == -1):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Invalid timedelta format: Every time value needs a unit[/color]", targetID)
                return
            elif(times == -2):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The last time value had no unit[/color]", targetID)
                return
            elif(type(times) == type("")):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]The time unit '{}' does not exist.[/color]\nAll available time units:\n'w' weeks | 'd' days | 'h' hours | 'm' minutes | 's' seconds | 'f' miliseconds".format(times), targetID)
                return


            if(len(parameters) == 2):
                self.timedCall(method, [schid, targetMode, targetID, senderName], parameters=[], times=times)
            else:
                self.timedCall(method, [schid, targetMode, targetID, senderName], parameters=parameters[2:], times=times)

            self.sendTextMessage(schid, targetMode, "[B]@'{}':[/B] [color=#00aa00]Timer started![/color]".format(senderName), targetID)


    def cmdTeams(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) == 0):
            parameters.append("-channel")

        teamCount = 2
        type = "list"
        clientList = []

        if(re.match(r"-\d+", parameters[0]) != None):
            teamCount = int(parameters[0][1:])
            del(parameters[0])

        if(teamCount == 1 or teamCount == 0):
            self.sendTextMessage(schid, targetMode, "[color=#bb0000]Why would you want only {} team? What are you, stupid?[/color]".format(teamCount), targetID)
            return
        elif(teamCount < 0):
            self.sendTextMessage(schid, targetMode, "[color=#bb0000]Why would you want a negative amount of teams? Retard...[/color]", targetID)
            return
        elif(teamCount > 10):
            self.sendTextMessage(schid, targetMode, "[color=#bb0000]??? No fuck you.[/color]", targetID)
            return


        if(parameters[0] in ("-channel", "-nomute-channel", "-nofullmute-channel", "-server", "-nomute-server", "-nofullmute-server")):
            type = parameters[0][1:]
            del(parameters[0])


        if(type == "list"):
            clientList = parameters
        else:
            if(type == "channel"):
                myChannelID = self.getMyChannelID(schid)
                (err, clientIDs) = ts3lib.getChannelClientList(schid, myChannelID)
                isInputRelevant = False
                isOutputRelevant = False

            elif(type == "nomute-channel"):
                myChannelID = self.getMyChannelID(schid)
                (err, clientIDs) = ts3lib.getChannelClientList(schid, myChannelID)
                isInputRelevant = True
                isOutputRelevant = True

            elif(type == "nofullmute-channel"):
                myChannelID = self.getMyChannelID(schid)
                (err, clientIDs) = ts3lib.getChannelClientList(schid, myChannelID)
                isInputRelevant = False
                isOutputRelevant = True

            elif(type == "server"):
                (err, clientIDs) = ts3lib.getClientList(schid)
                isInputRelevant = False
                isOutputRelevant = False

            elif(type == "nomute-server"):
                (err, clientIDs) = ts3lib.getClientList(schid)
                isInputRelevant = True
                isOutputRelevant = True

            elif(type == "nofullmute-server"):
                (err, clientIDs) = ts3lib.getClientList(schid)
                isInputRelevant = False
                isOutputRelevant = True

            for id in clientIDs:
                (err, name) = ts3lib.getClientVariableAsString(schid, id, ts3defines.ClientProperties.CLIENT_NICKNAME)
                (err, isInputMuted) = ts3lib.getClientVariableAsInt(schid, id, ts3defines.ClientProperties.CLIENT_INPUT_MUTED)
                (err, isOutputMuted) = ts3lib.getClientVariableAsInt(schid, id, ts3defines.ClientProperties.CLIENT_OUTPUT_MUTED)

                if((isInputRelevant == True and isInputMuted == 1) or (isOutputRelevant == True and isOutputMuted == 1)):
                    continue
                clientList.append(name)

        if(len(clientList) == 0):
            self.sendTextMessage(schid, targetMode, "[color=#bb0000]No targets found.[/color]", targetID)
            return

        teams = []
        for i in range(0, teamCount):
            teams.append([])

        teamIndex = 0
        for i in range(0, len(clientList)):
            index = randint(0, len(clientList)-1)
            teams[teamIndex].append(clientList[index])
            del(clientList[index])

            teamIndex = (teamIndex + 1) % teamCount

        teamStrings = ""
        for i in range(0, teamCount):
            team = teams[i]
            teamStrings += "\nTeam {}:\t{}".format(i+1, ",\t".join(team))


        self.statsIncrement("All", "Teams built", 1, 5)
        self.sendTextMessage(schid, targetMode, "[B]The teams are as follows:\n{}[/B]".format(teamStrings), targetID)


    def cmdPlaysound(self, schid, targetMode, targetID, senderName, parameters):
        if(self.psEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Playsounds are currently disabled.[/color]", targetID)
            return

        if(len(parameters) == 0):
            parameters.append("1")

        if(len(parameters) == 1): #list all playsounds
            try:
                pageNum = int(parameters[0])
                toPrint = ""

                firstDisplayed = (pageNum - 1) * self.psSoundsPerPage

                if(firstDisplayed > len(self.psSounds)):
                    pageNum = int(len(self.psSounds) / self.psSoundsPerPage) + 1
                    firstDisplayed = (pageNum - 1) * self.psSoundsPerPage

                lastDisplayed = self.min(len(self.psSounds), firstDisplayed + self.psSoundsPerPage)
                for i in range(firstDisplayed, lastDisplayed):
                    soundDisplayName = list(self.psSounds.keys())[i]
                    toPrint += "\n'{}'\t({} Pts.)".format(soundDisplayName, self.psSounds[soundDisplayName][1])

                toPrint += "\n\t\t\t[b]-\tPage ({}/{})\t-[/b]\n\nTo switch pages write '{}ps <1/2/3...>'".format(pageNum, int(len(self.psSounds) / self.psSoundsPerPage) + 1, self.chatCommandPrefix)

                self.sendLongTextMessage(schid, targetMode, "[b]Here is a list of all playsounds ({}):[/b]\n{}\n\n[b]Use a playsound:[/b] \"{}playsound [-modifier] <sound>\"".format(len(self.psSounds), toPrint, self.chatCommandPrefix), targetID, "\n")
                return
            except:
                if(parameters[0] == "-mods"):
                    self.sendLongTextMessage(schid, targetMode, "\n\n[b]The following modifiers are available:[/b]\n\n-earrape\t(10x Volume, [color=#aa0000]+100% cost[/color])\n-nc\t(125% Speed)\n-demon(70% Speed, 1.5x Volume, [color=#aa0000]+20% cost[/color])\n\n[b]Use a playsound:[/b] \"{}playsound [-modifier] <sound>\"".format(self.chatCommandPrefix), targetID, "\n")
                    return

        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.ecoIsBlacklisted(senderUID) or self.ecoIsExcluded(senderUID)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], excluded/blacklisted accounts can't use playsounds![/color]".format(senderName), targetID)
            return

        if(targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL and parameters[0] != "-add" and parameters[0] != "-source"):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry, this command only works in channels[/color]", targetID)
            return

        if(parameters[0].startswith("-")):
            if(parameters[0] == "-add"):    #!playsound -add sound.mp3 5 FUCK YOU
                if(self.privGetClientLevel(senderUID) != self.privMyLevel):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], you can't use that command.(2)[/color]".format(senderName), targetID)
                    return

                soundFileName = parameters[1]
                soundFileNoEnding = ".".join(soundFileName.split(".")[0:-1])

                if(not soundFileName.endswith(".mp3") and not soundFileName.endswith(".wav")):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]The audio type [b]'{}'[/b] is not compatible. Only .mp3 and .wav files![/color]".format("."+soundFileName.split("\\.")[-1]), targetID)
                    return

                if(not os.path.isfile(self.getPathToFile("playsounds")+"\\"+soundFileName)):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]The file [b]'{}'[/b] was not found in the '/playsounds/' folder.[/color]".format(soundFileName), targetID)
                    return

                for soundDisplayName in self.psSounds:
                    if(soundFileNoEnding.lower() == self.psSounds[soundDisplayName][0].lower()):
                        self.sendTextMessage(schid, targetMode, "[color=#cc0000]The file [b]'{}'[/b] already exists for the sound [b]'{}'[/b].[/color]".format(soundFileName, soundDisplayName), targetID)
                        return


                try:
                    soundCost = int(parameters[2])
                except:
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000][b]'{}'[/b] was not found in the '/playsounds/' folder.[/color]".format(parameters[2]), targetID)
                    return


                soundDisplayName = " ".join(parameters[3:])

                if(len(soundDisplayName) <= 1):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]The name [b]'{}'[/b] is too short. Sounds need to be at least 2 chars long.[/color]".format(soundDisplayName), targetID)
                    return

                for tempDisplayName in self.psSounds:
                    if(soundDisplayName.lower() == tempDisplayName.lower()):
                        self.sendTextMessage(schid, targetMode, "[color=#cc0000]The sound [b]'{}'[/b] already exists.[/color]".format(tempDisplayName), targetID)
                        return

                if(soundFileName.endswith(".mp3")):
                    self.convertMp3ToWav(self.getPathToFile("playsounds")+"\\"+soundFileName)


                self.psSounds[soundDisplayName] = [soundFileNoEnding, soundCost, ""]

                self.sendTextMessage(schid, targetMode, "[color=#00aa00]The sound [b]'{}'[/b] was successfully added.[/color]".format(soundDisplayName), targetID)
                return
            elif(parameters[0] == "-print"):
                if(self.privGetClientLevel(senderUID) < self.privAdminLevel):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]{}[/B], you can't use that command.(3)[/color]".format(senderName), targetID)
                    return

                name = " ".join(parameters[1:])
                foundChannelID = self.getChannelIDByName(schid, name)

                if(foundChannelID == -1):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]No channel containing [b]'{}'[/b] was found.[/color]".format(name), targetID)

                elif(type(foundChannelID) == type([])):
                    self.sendTextMessage(schid, targetMode, "[color=#cc0000]Multiple channel containing [b]'{}'[/b] found:\n[b]{}[/b][/color]".format(name, "\n".join(foundChannelID)), targetID)

                else:
                    toPrint = "[b]All playsounds:[/b]\n"
                    for i in range(0, len(self.psSounds)):
                        soundDisplayName = list(self.psSounds.keys())[i]
                        toPrint += "\n'{}'\t({} Pts.)".format(soundDisplayName, self.psSounds[soundDisplayName][1])
                        if(i % self.psSoundsPerPage == self.psSoundsPerPage-1):
                            toPrint += "\n"

                    toPrint += "\n\n[b]The following modifiers are available:[/b]\n"
                    toPrint += "-earrape\t(10x Volume, [color=#cc0000]+100%[/color] Cost)\n"
                    toPrint += "-nc\t(125% Speed, [color=#00aa00]-20%[/color] Cost)\n"
                    toPrint += "-demon\t(70% Speed, 1.5x Volume, [color=#cc0000]+42%[/color] Cost)\n"

                    toPrint += "\n[b]Use a playsounds:[/b] \"{}playsound [-modifier] <sound>\"".format(self.chatCommandPrefix)
                    toPrint += "\n\nUpdated {}".format(datetime.now().strftime("%d.%m.%y"))

                    #self.debug("toPrint:\n\n{}".format(toPrint))

                    ts3lib.setChannelVariableAsString(schid, foundChannelID, ts3defines.ChannelProperties.CHANNEL_DESCRIPTION, toPrint)
                    ts3lib.flushChannelUpdates(schid, foundChannelID)

                    self.sendTextMessage(schid, targetMode, "[color=#00aa00]Printed all playsounds to the channel description of [b]'{}'[/b].[/color]".format(self.getChannelNameByID(schid, foundChannelID)), targetID)

                return


                                                      #and not self.isMeByUID(senderUID)
        if(self.hasCooldown(senderUID, "cmd:playsound")):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]That command is still on cooldown! ([B]{}[/B] cooldown total)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:playsound")), targetID)
            return

        if(parameters[0].startswith("-") and len(parameters) >= 2):
            if(parameters[0].lower() == "-earrape"):
                modifier = "earrape"
            elif(parameters[0].lower() == "-nc"):
                modifier = "nightcore"
            elif(parameters[0].lower() == "-demon"):
                modifier = "demon"
            elif(parameters[0].lower() == "-source"):
                modifier = "source"
            elif(parameters[0].lower() == "-setsource"):
                if(self.privGetClientLevel(senderUID) < self.privModLevel):
                    self.sendTextMessage(schid, targetMode, "[color=#aa0000]You can't use the parameter 'setsource', [b]\'{}\'[/b]![/color]".format(senderName), targetID)
                    return
                modifier = "source"
                url = parameters[1]
                if(url.startswith("[URL]") and url.endswith("[/URL]")):
                    url = url[5:-6]
                del(parameters[1])
            elif(parameters[0].lower() == "-setcost"):
                if(self.privGetClientLevel(senderUID) < self.privModLevel):
                    self.sendTextMessage(schid, targetMode, "[color=#aa0000]You can't use the parameter 'setcost', [b]\'{}\'[/b]![/color]".format(senderName), targetID)
                    return
                modifier = "setcost"
                newCost = parameters[1]
                del(parameters[1])
            else:
                self.sendTextMessage(schid, targetMode, "[color=#aa0000][b]\"{}\"[/b] is not a valid modifier[/color]".format(parameters[0]), targetID)
                return

            sound = " ".join(parameters[1:])
        else:
            modifier = "none"
            sound = " ".join(parameters)


        if(sound.lower() == "random"):
            randIndex = randint(0, len(self.psSounds) - 1)
            soundDisplayName = list(self.psSounds.keys())[randIndex]
        else:
            exactSound = ""
            foundSounds = []

            for soundDisplayName in self.psSounds:
                if(soundDisplayName.lower() == sound.lower()):
                    exactSound = soundDisplayName
                elif(sound.lower() in soundDisplayName.lower()):
                    foundSounds.append(soundDisplayName)

            if(exactSound == "" and len(foundSounds) == 0):
                self.sendTextMessage(schid, targetMode, "[color=#aa0000]No sound found with [b]\"{}\"[/b] in its name[/color]".format(sound), targetID)
                return
            elif(exactSound == "" and len(foundSounds) > 1):
                self.sendTextMessage(schid, targetMode, "[color=#aa0000]Found multiple sounds with [b]\"{}\"[/b] in their name: [b]'{}'[/b][/color]".format(sound, ", ".join(foundSounds)), targetID)
                return


            if(exactSound != ""):
                soundDisplayName = exactSound
            else:
                soundDisplayName = foundSounds[0]


        targetSoundFileName = self.psSounds[soundDisplayName][0]
        targetSoundCost = self.psSounds[soundDisplayName][1]

        playbackSpeed = self.psDefaultSpeed
        playbackVolume = self.psDefaultVolume
        cooldown = "10s"

        if(modifier == "earrape"):
            playbackVolume = playbackVolume * 10
            targetSoundCost = math.ceil(targetSoundCost * 2)
            cooldown = "15s"
        elif(modifier == "nightcore"):
            playbackSpeed = playbackSpeed * 1.25
            targetSoundCost = math.ceil(targetSoundCost * 0.8)
        elif(modifier == "demon"):
            playbackSpeed =  playbackSpeed * 0.7
            playbackVolume = playbackVolume * 1.5
            targetSoundCost = math.ceil(targetSoundCost * 1.42)
        elif(modifier == "source"):
            if("url" in locals()):     #[URL]http://www.google.com[/URL]
                self.psSounds[soundDisplayName][2] = url
                self.sendTextMessage(schid, targetMode, "[color=#00aa00]Source of sound [b]'{}'[/b] has been set to {}[/color]".format(soundDisplayName, url, url), targetID)
            else:
                source = self.psSounds[soundDisplayName][2]
                if(source != ""):
                    if(source.startswith("http")):
                        self.sendTextMessage(schid, targetMode, "Source of sound [b]'{}'[/b]: [url={}]{}[/url]".format(soundDisplayName, source, source), targetID)
                    else:
                        self.sendTextMessage(schid, targetMode, "Source of sound [b]'{}'[/b]: {}".format(soundDisplayName, source, source), targetID)
                else:
                    self.sendTextMessage(schid, targetMode, "The sound [b]'{}'[/b] does not have a source attached.".format(soundDisplayName), targetID)
            return
        elif(modifier == "setcost"):
            try:
                newCost = int(newCost)
            except:
                pass
            self.psSounds[soundDisplayName][1] = newCost
            self.sendTextMessage(schid, targetMode, "[color=#00aa00]Cost of sound [b]'{}'[/b] set to[/color] [color=#000099][b]{}[/b][/color]".format(soundDisplayName, newCost), targetID)
            return


        if(not self.ecoCanPay(senderUID, targetSoundCost)):
            modStr = ""
            if(modifier != "none"):
                modStr = " + {}".format(modifier)

            self.sendTextMessage(schid, targetMode, "[color=#cc0000]You don't have enough points for [b]'{}{}'[/b]. Your balance: [b]'{}'[/b] : [b]{}[/b] cost[/color]".format(soundDisplayName, modStr, self.ecoGetBalance(senderUID), targetSoundCost), targetID)
            return

        self.play_wav(self.getPathToFile("playsounds")+"/"+targetSoundFileName+".wav", playbackSpeed, playbackVolume)

        self.ecoChangeBalance(senderUID, -targetSoundCost)
        self.setCooldown(senderUID, "cmd:playsound", cooldown)
        self.statsIncrement("Playsounds", "Total sounds played", 1, 0)
        self.statsIncrement("Playsounds", "Total balance wasted", targetSoundCost, 1)
        self.statsIncrement("Playsounds", "Sound '{}'".format(soundDisplayName), 1, 99)
        self.sendTextMessage(schid, targetMode, "[b]@{}:[/b] [color=#006600]Playing sound [b]'{}'[/b].[/color] Your balance: {} Pts. [color=#aa0000](-{})[/color].".format(senderName, soundDisplayName, self.ecoGetBalance(senderUID), targetSoundCost), targetID)

        return


    def cmdTick(self, schid, targetMode, targetID, senderName, parameters):
        now = datetime.now()

        addedTimeStart = ((self.ecoStartTime.day * 24 + self.ecoStartTime.hour) * 60 + self.ecoStartTime.minute) * 60 + self.ecoStartTime.second
        addedTimeNow = ((now.day * 24 + now.hour) * 60 + now.minute) * 60 + now.second

        timeDiff = addedTimeNow - addedTimeStart
        timeDiff = self.ecoTickTimeInSeconds - (timeDiff % self.ecoTickTimeInSeconds)

        timeString = self.formatSecondsToTimeString(timeDiff, mode=1)

        self.sendTextMessage(schid, targetMode, "Time left until the next point: [b]{}[/b]".format(timeString), targetID)

        #self.printTime(self.ecoStartTime)
        #self.printTime(now)

    def printTime(self, time):
        self.debug("Year: {}, Month: {}, Day: {}, Hour: {}, Minute: {}, Second: {}".format(time.year, time.month, time.day, time.hour, time.minute, time.second))


    def cmdStats(self, schid, targetMode, targetID, senderName, parameters):
        reqModule = "*"

        if(len(parameters) == 0): #print all stats of all modules
            pass
        elif(len(parameters) == 1): #print only of certain module
            reqModule = parameters[0]
        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("stats"), targetID)
            return


        if(reqModule == ""):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]No module given.[/color]", targetID)
            return



        retString = ""
        listModules = []

        if(reqModule == "*"):
            for module in self.statsData:
                listModules.append(module)

        elif(reqModule != "*"):
            if(not self.statsModuleExists(reqModule)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Module '{}' doesn't exist.[/color]".format(module), targetID)
                return
            listModules.append(reqModule)


        for module in listModules:
            retString += "\r\n[B]["+module+"]:[/B]\n"
            fields = sorted(self.statsData[module], key=lambda x: (self.statsData[module][x][1], [x]))
            for field in fields:
                count = self.statsGet(module, field)
                retString += "\t- {}: {}\n".format(field, count)


        self.sendLongTextMessage(schid, targetMode, retString, targetID)



    def cmdDaily(self, schid, targetMode, targetID, senderName, parameters):
        if(self.dailyEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Daily rewards are currently disabled.[/color]", targetID)
            return

        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.ecoIsWhitelisted(senderUID) == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], you have to be whitelisted to use daily rewards![/color]".format(senderName), targetID)
            return

        senderBalance = self.ecoGetBalance(senderUID)
        timeLeft = self.dailyGetTimeLeft(senderUID)

        if(timeLeft != None):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]You already collected this reward! Next reward: [b]{}[/b][/color]".format(timeLeft), targetID)
            return

        if(senderBalance + self.dailyReward > self.ecoPointLimit):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]You would have more than [b]{} Pts.[/b], [b]'{}'[/b][/color]. Your balance: [b]{} Pts.[/b], daily reward: [b]{} Pts.[/b]".format(self.ecoPointLimit, senderName, senderBalance, self.dailyReward), targetID)
            return

        self.dailySetOnCooldown(senderUID)
        self.statsIncrement("All", "Daily balance recieved", self.dailyReward, 2)
        self.sendTextMessage(schid, targetMode, "[color=#00aa00][b]{} Pts.[/b] have been added to your balance as daily reward, [b]'{}'[/b][/color]".format(self.dailyReward, senderName), targetID)
        newBalance = self.min(self.ecoPointLimit * 2, self.ecoGetBalance(senderUID) + self.dailyReward)
        self.ecoSetBalance(senderUID, newBalance)



    def cmdReward(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.ecoIsWhitelisted(senderUID) == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], you have to be whitelisted claim rewards![/color]".format(senderName), targetID)
            return

        if(self.rewardsEnabled == False and not self.isMeByUID(senderUID)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Rewards are currently disabled.[/color]", targetID)
            return

        if(len(parameters) != 1):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("reward"), targetID)
            return


        ret = self.useCode(parameters[0], senderUID, senderName)

        if(ret[0] == -1):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]That reward does not exist, [b]'{}'[/b]![/color]".format(senderName), targetID)
            return

        elif(ret[0] == -2):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]That reward has already expired, [b]'{}'[/b]![/color]".format(senderName), targetID)
            return

        elif(ret[0] == -3):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]You have too many Pts. to claim this reward, [b]'{}'[/b]! ([b]{}[/b] -> [b]{}[/b])[/color]".format(senderName, self.ecoGetBalance(senderUID), self.ecoGetBalance(senderUID) + ret[1][3]), targetID)
            return

        elif(ret[0] == -4):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]That reward has already been claimed by [b]'{}'[/b] ({})[/color]".format(ret[1][6], ret[1][5]), targetID)
            return

        elif(ret[0] == -5):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000]You already claimed this reward, [b]'{}'[/b]![/color]".format(senderName), targetID)
            return

        self.sendTextMessage(schid, targetMode, "[color=#00aa00]Reward claimed! You recieved [b]{} Pts.[/b] ([b]{}[/b] -> [b]{}[/b])[/color]".format(ret[1][3], self.ecoGetBalance(senderUID) - ret[1][3], self.ecoGetBalance(senderUID)), targetID)

        args = ret[1][4]

        if(len(args) != 0):
            if(args[0] == "play"):
                self.play_wav(self.getPathToFile("playsounds")+"/"+args[1]+".wav")
        return



    def cmdMakereward(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)


        if(parameters[0].lower() == "single" or parameters[0].lower() == "single-use" or parameters[0].lower() == "single_use"):
            type = "single-use"
        elif(parameters[0].lower() == "unlimited" or parameters[0].lower() == "unlimited-use" or parameters[0].lower() == "unlimited_use"):
            type = "unlimited-use"
        else:
            self.sendTextMessage(schid, targetMode, "[color=#aa0000][b]'{}'[/b] is not one of (single, single-use, multi, multi-use)[/color]".format(parameters[0]), targetID)
            return

        try:
            reward = int(parameters[2])
            if(reward <= 0):
                raise ValueError("Integer not greater than 0");
        except:
            self.sendTextMessage(schid, targetMode, "[color=#aa0000][b]'{}'[/b] is not a valid integer (>0)[/color]".format(parameters[0]), targetID)
            return


        times = self.getTimesFromTimeString(parameters[1])
        if(times == -1):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000][b]'{}'[/b] is not a valid integer (>0)[/color]".format(parameters[0]), targetID)
            return
        elif(times == -2):
            self.sendTextMessage(schid, targetMode, "[color=#aa0000][b]'{}'[/b] is not a valid integer (>0)[/color]".format(parameters[0]), targetID)
            return

        if(len(parameters) < 4):
            params = "-"
        else:
            params = " ".join(parameters[3:])

        #error handling for wrong input here
        code = self.createCode(type, times, reward, params)

        codeInfo = self.getCodeInfo(code)
        self.debug("codeInfo '{}'".format(codeInfo))
        self.sendTextMessage(schid, targetMode, "[color=#00aa00]Reward successfully created.[/color]\nInfo about the reward:\nCode:\t[b]{}[/b]\nType:\t{}\nExpiration Date:\t{}\nReward Amount:\t{} Pts.".format(codeInfo[0], codeInfo[1], codeInfo[2].strftime("%Y.%m.%d %H:%M:%S"), codeInfo[3]), targetID)



    def cmdSetbalance(self, schid, targetMode, targetID, senderName, parameters):#  !setbalance <nameFrag> [~]<value>
        if(len(parameters) != 2):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("setbalance"), targetID)
            return

        targetNamePart, value = parameters[0], parameters[1]
        targetName = self.getClientNameFromSubName(schid, targetNamePart)

        if(targetName == -1):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]No user with [B]'{}'[/B] in their name was found.[/color]".format(targetNamePart), targetID)
            return
        if(type(targetName) == type([])):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B][/color]".format(", ".join(targetName)), targetID)
            return

        targetUID = self.getClientUIDByName(schid, targetName)


        if(value.startswith("~")):
            try:
                relativeValueInt = int(value[1:])
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] doesn't seem to be a valid number.[/color]".format(value[1:]), targetID)
                return
            setValue = self.max(self.ecoGetBalance(targetUID)+relativeValueInt, 0)


        else:
            try:
                valueInt = int(value)
            except:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000][B]'{}'[/B] doesn't seem to be a valid number.[/color]".format(value[1:]), targetID)
            setValue = self.max(valueInt, 0)

        self.ecoSetBalance(targetUID, setValue)
        self.sendTextMessage(schid, targetMode, "Successfully set [B]'{}'[/B]s balance to [B]'{}'[/B]".format(targetName, setValue), targetID)


    def cmdToken(self, schid, targetMode, targetID, senderName, parameters):
        if(self.tokenEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Tokens are currently disabled.[/color]", targetID)
            return

        if(len(parameters) == 0):
            copiedListTokens = copy.deepcopy(self.tokenListTokens)
            orderedTokenDict = collections.OrderedDict(sorted(copiedListTokens.items()))
            toPrint = ""
            for key, value in orderedTokenDict.items():
                if(toPrint == ""):
                    toPrint = "'{}'\t({} Pts.)\t-\t{}".format(key, value[1], value[0])
                else:
                    toPrint += "\n'{}'\t({} Pts.)\t-\t{}".format(key, value[1], value[0])
            self.sendTextMessage(schid, targetMode, "All available tokens:\n\n{}\n\nUse a token with '{}token <tokenName> <targetName>' (Hint: A part of the targets name is enough)".format(toPrint, self.chatCommandPrefix), targetID)

        elif(len(parameters) == 1 and parameters[0] == "unstuck"):#!token unstuck
            for timer in self.tokenTimers:
                listParams = self.tokenTimers[timer]
                clientName = self.getClientNameByUID(schid, listParams[1][3])

                if(clientName == senderName):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]You can't use unstuck, as the effect is still valid [B]@'{}'[/B]![/color]".format(senderName), targetID)
                    return

            self.tokenUnstuck = [schid, targetMode, targetID, senderName]
            ts3lib.requestServerGroupClientList(schid, self.tokenGagGroupID, True)
            ts3lib.requestServerGroupClientList(schid, self.tokenStickyGroupID, True)
            ts3lib.requestServerGroupClientList(schid, self.tokenMuteGroupID, True)

        elif(len(parameters) >= 1):
            tokenName = parameters[0].lower()
            senderUID = self.getClientUIDByName(schid, senderName)

            if(self.ecoIsBlacklisted(senderUID) or self.ecoIsExcluded(senderUID)):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], excluded/blacklisted accounts can't use tokens![/color]".format(senderName), targetID)
                return

            found = False
            for key in self.tokenListTokens:
                if(key.lower() == tokenName):
                    tokenName = key
                    found = True

            if(found == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That token does not exist, [B]'{}'[/B]. Type '{}token' for a list of all tokens[/color]".format(senderName, self.chatCommandPrefix), targetID)
                return

            if(self.hasCooldown(senderUID, "token:{}".format(tokenName))):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]That token is still on cooldown, [B]'{}'[/B]. Cooldown of [B]'{}'[/B]: [B]'{}'[/B][/color]".format(senderName, tokenName, self.getRemainingCooldownAsString(senderUID, "token:{}".format(tokenName))), targetID)
                return

            tokenPrice = self.tokenListTokens[tokenName][1]

            if(self.ecoCanPay(self.getClientUIDByName(schid, senderName), tokenPrice) == False):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You are missing [B]'{} Pts'[/B] for this token, @[B]'{}'[/B]. Price: [B]'{} Pts'[/B][/color]".format(tokenPrice - self.ecoGetBalance(senderUID), senderName, tokenPrice), targetID)
                return


            tokenParams = self.tokenParameters[tokenName]
            if(tokenParams[1] == "target"):
                targetNamePart = " ".join(parameters[1:])

                targetName = self.getClientNameFromSubName(schid, targetNamePart)
                if(targetName == -1):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]No user with [B]'{}'[/B] in their name was found.[/color]".format(targetNamePart), targetID)
                    return
                if(type(targetName) == type([])):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Multiple users found: [B]'{}'[/B][/color]".format(", ".join(targetName)), targetID)
                    return

                tokenTargetID = self.getClientIDByName(schid, targetName)
                tokenTargetUID = self.getClientUIDByName(schid, targetName)
                senderID = self.getClientIDByName(schid, senderName)

                if(self.ecoIsExcluded(tokenTargetUID)):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], [B]'{}'[/B] is excluded from the economy system and can't be targeted with tokens![/color]".format(senderName, targetName), targetID)
                    return

                (err, senderChannelID) = ts3lib.getChannelOfClient(schid, senderID)

                if("!!" in ts3lib.getChannelVariableAsString(schid, senderChannelID, ts3defines.ChannelProperties.CHANNEL_NAME)[1]):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], you are inside a protected channel and can't use tokens![/color]".format(senderName), targetID)
                    return

                (err, targetChannelID) = ts3lib.getChannelOfClient(schid, tokenTargetID)

                if("!!" in ts3lib.getChannelVariableAsString(schid, targetChannelID, ts3defines.ChannelProperties.CHANNEL_NAME)[1]):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry [B]'{}'[/B], [B]'{}'[/B] is inside a protected channel and can't be targeted with tokens![/color]".format(senderName, targetName), targetID)
                    return



            #for timer in self.tokenTimers:
            #    listParams = self.tokenTimers[timer]
            #    clientName = self.getClientNameByUID(schid, listParams[1][3])
            #
            #    if(clientName == targetName):
            #        self.sendTextMessage(schid, targetMode, "[color=#ff0000]A user can only have 1 token effect at a time[/color]", targetID)
            #        return


            isRightToken = False
            answerMessage = ""
            messageToTarget = ""

            if(tokenName == "cKick"):
                ts3lib.requestClientKickFromChannel(schid, self.getClientIDByName(schid, targetName), "Kicked via token by '{}'".format(senderName))
                messageToTarget = "[B]'{}'[/B] used a channel kick token on you".format(senderName)
                answerMessage = "[B]'{}'[/B] has been kicked from the channel by [B]'{}'[/B]".format(targetName, senderName)

            elif(tokenName == "sKick"):
                ts3lib.requestClientKickFromServer(schid, self.getClientIDByName(schid, targetName), "Kicked via token by '{}'".format(senderName))
                answerMessage = "[B]'{}'[/B] has been kicked from the server by [B]'{}'[/B]".format(targetName, senderName)

            elif(tokenName == "shoot"):
                num = randint(0, 500)
                if(num <= 100):
                    ts3lib.requestClientKickFromServer(schid, self.getClientIDByName(schid, targetName), "Shot via token (by '{}')".format(senderName))
                    answerMessage = "[B]'{}'[/B] has been shot dead by [B]'{}'[/B]".format(targetName, senderName)
                else:
                    if(num == 420):
                        ts3lib.requestClientKickFromServer(schid, self.getClientIDByName(schid, senderName), "Killed via token by yourself (idiot lol)")
                        answerMessage = "[B]'{}'[/B] is a complete moron and shot himself while trying to shoot [B]'{}'[/B]. LUL".format(senderName, targetName)
                    else:
                        answerMessage = "[B]'{}'[/B] tries to shoot [B]'{}'[/B] but failed miserably".format(senderName, targetName)

            elif(tokenName.startswith("gag")):#timedCall(self, method, cmdParameters, parameters, times, timerStorage=None)
                isRightToken = True
                rightGroupID = self.tokenGagGroupID
                answerMessage = "[B]'{}'[/B] has been gagged for [B]'{}'[/B] by [B]'{}'[/B]".format(targetName, self.tokenListTokens[tokenName][3], senderName)

            elif(tokenName.startswith("sticky")):
                isRightToken = True
                rightGroupID = self.tokenStickyGroupID
                answerMessage = "[B]'{}'[/B] has been stickied for [B]'{}'[/B] by [B]'{}'[/B]".format(targetName, self.tokenListTokens[tokenName][3], senderName)

            elif(tokenName.startswith("mute")):
                isRightToken = True
                rightGroupID = self.tokenMuteGroupID
                answerMessage = "[B]'{}'[/B] has been muted for [B]'{}'[/B] by [B]'{}'[/B]".format(targetName, self.tokenListTokens[tokenName][3], senderName)

            elif(tokenName == "banPerm"):
                listMessages = [[" - Really?", 1],
                                [" - Do you hate {} that much?".format(targetName), 2.5],
                                [" - I know it hasn't always been easy between you two,", 5],
                                [" - but I'm sure you had a ton of great moments together.", 2.5],
                                [" - And that's how it is supposed to end? PepeHands", 2.5],
                                [" - No! You two are a team! You belong together!.", 5],
                                [" - Together nothing can beat you!", 2.5],
                                [" - [B]I believe in you![/B]", 2.5],
                                [" - [B]BibleThump[/B]", 5]]
                self.timedCall(self.textShot, [schid, targetMode, targetID, senderName], [listMessages, 0], times=[0, listMessages[0][1], 0, 0, 0, 0], timerStorage=self.textTimers)
                self.ecoChangeBalance(senderUID, tokenPrice)

            elif(tokenName == "clear"):
                answerMessage = ""

                for i in range(0, 400):
                    answerMessage += "\n"

                answerMessage += "[b]Chat cleared by [color=#0000ff]{}[/color][/b]".format(senderName)

            elif(tokenName.startswith("poke")):
                messageToTarget = "[B]'{}'[/B] used an annoying poke ({}x1 per second) token on you".format(senderName, self.tokenListTokens[tokenName][3])
                answerMessage = "[B]'{}'[/B] ís being poked [B]'{}x1 per Second'[/B] by [B]'{}'[/B]".format(targetName, self.tokenListTokens[tokenName][3], senderName)
                ts3lib.requestClientPoke(schid, tokenTargetID, "Annoying poke ({}) by '{}'".format(1, senderName))
                self.timedCall(self.pokeShot, [schid, targetMode, targetID, senderName], [tokenName, targetName, self.tokenListTokens[tokenName][3], 1, 1], times=[0, 1, 0, 0, 0, 0], timerStorage=self.pokeTimers)

            else:
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry, [B]'{}'[/B], that feature is not yet implemented![/color]".format(senderName), targetID)
                return


            #For all tokens using the rights system
            if(isRightToken == True):
                if(rightGroupID == 0):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Token can't be executed as the GroupID hasn't been defined for this group![/color]".format(targetName), targetID)
                    return
                elif(self.tokenOperationParams != []):
                    self.sendTextMessage(schid, targetMode, "[color=#ff0000]Something went terribly wrong here, or my Internet just sucks. (ErrorCode: 1)[/color]", targetID)
                    return

                self.tokenOperationParams = [schid, tokenName, tokenTargetID, targetName, senderName]
                ts3lib.requestClientDBIDfromUID(schid, self.getClientUIDByName(schid, targetName))


            self.ecoChangeBalance(senderUID, tokenPrice * -1)
            self.setCooldown(senderUID, "token:{}".format(tokenName), self.tokenListTokens[tokenName][2])

            if(tokenName.lower() == "ckick"):
                self.setCooldown(senderUID, "token:sticky1", self.tokenListTokens["sticky1"][2])
                self.setCooldown(senderUID, "token:sticky2", self.tokenListTokens["sticky2"][2])
                self.setCooldown(senderUID, "token:sticky3", self.tokenListTokens["sticky3"][2])

            if(tokenName.lower().startswith("sticky")):
                self.setCooldown(senderUID, "token:cKick", self.tokenListTokens[tokenName][2])


            if(messageToTarget != ""):
                self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, messageToTarget, self.getClientIDByName(schid, targetName))
            if(answerMessage != ""):
                self.sendTextMessage(schid, targetMode, answerMessage, targetID)

        else:
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("token"), targetID)



    def cmdTts(self, schid, targetMode, targetID, senderName, parameters):
        if((len(parameters) < 1 or (parameters[0].startswith("-") and len(parameters) < 2)) and parameters[0] != "-setup"):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("tts"), targetID)
            return

        if(targetMode != ts3defines.TextMessageTargetMode.TextMessageTarget_CHANNEL and parameters[0] != "-setup" and False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry, this command only works in channels[/color]", targetID)
            return

        senderUID = self.getClientUIDByName(schid, senderName)

        if(self.ecoIsExcluded(senderUID) == True or self.ecoIsBlacklisted(senderUID) == True):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry @[B]'{}'[/B], excluded users can't use this command.[/color]", targetID)
            return

        if(self.ttsEnabled == False):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Text-To-Speech is currently disabled.[/color]", targetID)
            for char in " ".join(parameters[1:]).replace(" ", ""):
                if(ord(char) == 65039 or ord(char) == 8205):
                    continue
                self.debug("char '{}' -> '{}' with len({})".format(char, ord(char), len(str(char))))
            return
        elif((self.ttsGlobalOutput == -1 or self.ttsLocalOutput == -1) and parameters[0] != "-setup"):
            self.sendTextMessage(schid, targetMode, """[color=#ff0000]Global/local audio output is undefined. Here is how you set it up:[/color]\n1.)
                                 Type '{}tts -setup'\n2.) Find your global and local audio output (Global: Everyone can hear, Local:
                                 Only you can hear)\n -> Hint: The '>' means default audio input and '<' means default audio output\n3.)
                                 Set the respective variables with '{}setvar ttsGlobalOutput <ID>'and '{}setvar ttsLocalOutput <ID>'""".format(self.chatCommandPrefix, self.chatCommandPrefix, self.chatCommandPrefix), targetID)
            return
        elif(parameters[0] == "-setup"):
            self.textToSpeech("", setup=True)
            return

        if(self.hasCooldown(senderUID, "cmd:tts")):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]That command is still on cooldown! ([B]{}s[/B] cooldown total)[/color]".format(self.getRemainingCooldownAsString(senderUID, "cmd:tts")), targetID)
            return
        if(hasattr(self.ttsUserCD, senderUID)):
            del(self.ttsUserCD[senderUID])

        language = "detect"
        translateTo = "none"

        if(parameters[0].startswith("-") and not parameters[0].startswith("-t-")):
            if(len(parameters[0]) < 3):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You have to enter a language when starting with a dash '-'[/color]", targetID)
                return
            language = parameters[0][1:]
            del(parameters[0])
        elif(parameters[0].startswith("-t-")):
            if(len(parameters[0]) < 5):
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]You have to enter a language when starting with a dash '-t-'[/color]", targetID)
                return
            translateTo = parameters[0][3:]
            del(parameters[0])

        message = " ".join(parameters)
        amountChars = len(message.replace(" ", ""))



        if(amountChars > self.ttsMaxChars):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry @[B]'{}'[/B], that message is too long. ([B]{}[/B] character maximum)[/color]".format(senderName, self.ttsMaxChars), targetID)
            return

        #Measured using various unicode characters 2**x and TeamSpeak chat
        oneCharCutoff = 2**7
        twoCharsCutoff = 2**11
        threeCharsCutoff = 2**16

        #self.debug("amountChars before -> {}".format(amountChars))

        for char in message.replace(" ", ""):
            if(ord(char) < oneCharCutoff):
                pass
            elif(ord(char) < twoCharsCutoff):
                amountChars += self.ttsUnicodeIncrease * 1
            elif(ord(char) < threeCharsCutoff):
                amountChars += self.ttsUnicodeIncrease * 2
            else:
                amountChars += self.ttsUnicodeIncrease * 3

        #self.debug("amountChars after -> {}".format(amountChars))

        cost = self.max(int(amountChars / self.ttsCharsPerPoint), 1)
        #self.debug("cost: '{}' len(message) '{}' ttsCharsPerPoint '{}' division '{}'".format(cost, len(message), self.ttsCharsPerPoint, int(len(message) / self.ttsCharsPerPoint)))


        if(not self.ecoCanPay(senderUID, cost)):
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Sorry @[B]'{}'[/B], you don't have enough points for that message. (cost: [B]{}[/B] at '{}' characters per point)[/color]".format(senderName, cost, self.ttsCharsPerPoint), targetID)
            return
        self.ecoChangeBalance(senderUID, -cost)

        cooldownInSeconds = int(amountChars * self.ttsCDPerChar)
        cooldownInSeconds = self.min(self.ttsMaxCD, self.max(self.ttsMinCD, cooldownInSeconds))
        self.ttsUserCD[senderUID] = cooldownInSeconds

        self.setCooldown(senderUID, "cmd:tts", "{}s".format(cooldownInSeconds))
        self.statsIncrement("Text-To-Speech", "Total balance wasted", cost, 0)
        self.statsIncrement("Text-To-Speech", "Characters spoken", amountChars, 1)

        self.textToSpeech(message, language=language, translateTo=translateTo)




    def cmdWatch2gether(self, schid, targetMode, targetID, senderName, parameters):
        senderUID = self.getClientUIDByName(schid, senderName)
        if(self.hasCooldown(senderUID, "cmd:w2g")):
            return

        self.setCooldown(senderUID, "cmd:w2g", "5m")

        r = requests.post("http://www.vi-home.de/w2g/create.php?v=sZxpJv7Ir4E", data={})

        if(r.status_code == 200):
            self.sendTextMessage(schid, targetMode, "Here is your room: [url={}]{}[/url]".format(r.url, r.url), targetID)
        else:
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Shits fucked up yo (some error while fetching the url happened)[/color]", targetID)


    def cmdFloodprevent(self, schid, targetMode, targetID, senderName, parameters):
        if(len(parameters) != 1 or (parameters[0] != "poke" and parameters[0] != "private_chat" and parameters[0] != "command")):
            self.sendTextMessage(schid, targetMode, self.getUnknownParamsMessage("floodprevent"), targetID)
            return

        allSuppressed = self.fpSupressed[parameters[0]]

        if(len(allSuppressed) == 0):
            self.sendTextMessage(schid, targetMode, "[b]No entries in the flood prevention.[/b]", targetID)
            return

        if(parameters[0] == "poke"):
            action = "pokes"
        elif(parameters[0] == "private_chat"):
            action = "private messages"
        elif(parameters[0] == "command"):
            action = "command requests"

        retString = ""

        for UID in allSuppressed:
            times = allSuppressed[UID]
            if(times == 0):
                continue

            name = self.getClientNameByUID(schid, UID)

            name = UID if name == -1 else name

            retString += "\n\t-\tSuppressed [b]'{}'[/b] {} from [b]'{}'[/b]".format(times, action, name)

        if(retString == ""):
            self.sendTextMessage(schid, targetMode, "[b]No entries in the flood prevention.[/b]", targetID)
            return
        else:
            self.sendTextMessage(schid, targetMode, "[b]Entries in the flood prevention:[/b]\n{}".format(retString), targetID)
            return






    def cmdOptions(self, schid, targetMode, targetID, senderName, parameters):
        #Commands should be:
        #!options -> list of all modules and, if they are enabled, all variables in the modules
        #!options <moduleName> -> explanation of the module and all variables in the module
        #!options <varName> <value...> -> set a variable to a given value (disallow setting different type for variable!)
        #
        #default is ["variableInModule", "explanation", "variableName -> variableValue", "'variableName' set to 'newVariableValue'"]
        #           |______________________________________________________________________________________________________________|
        #                                                              |
        #                                                     _________|_________
        #                                                    |                   |
        #self.optionsDict["moduleName"] = ["enableVarName", [["chatCommandPrefix"]], "moduleExplanation"]

        if(len(parameters) == 0):
            retMessage = "Here is a list of all available options:"
            retMessageLong = ""

            for moduleName, moduleList in self.optionsDict.items():
                isEnabled = getattr(self, moduleList[0])
                moduleVars = moduleList[1]
                moduleExplanation = moduleList[2]
                #self.debug("moduleName '{}' moduleVars '{}' moduleExplanation '{}'\n\n".format(moduleName, moduleVars, moduleExplanation))
                #self.debug("")

                #retMessage += "\n\n--- [{}] => [color={}]{}[/color]".format(moduleName, self.optionsColorBoolTrue if isEnabled else self.optionsColorBoolFalse, "Enabled" if isEnabled else "Disabled")
                retMessage += "\n\n--- [[color={}]{}[/color]]".format(self.optionsColorBoolTrue if isEnabled else self.optionsColorBoolFalse, moduleName)

                if(isEnabled == True):
                    if(len(moduleVars) == 0):
                        retMessage += "\nNo module variables."
                    else:
                        retMessage += "\nModule variables:".format(moduleList[0])
                        for varList in moduleVars:
                            varName = varList[0]
                            varVal = getattr(self, varName)

                            base = " - !var! -> "

                            if(len(varList) == 2):
                                baseStatusMessage = "{}[color=!colorCode!]!val![/color]".format(base)
                            elif(len(varList) == 4):
                                baseStatusMessage = "{}{}".format(base, varList[2])
                            else:
                                baseStatusMessage = "Something went terribly wrong here..."

                            baseStatusMessage = "\n{}".format(baseStatusMessage)

                            baseStatusMessage = baseStatusMessage.replace("!var!", varName)
                            if(type(varVal) == type(True)):
                                baseStatusMessage = baseStatusMessage.replace("!colorCode!", self.optionsColorBoolTrue if varVal else self.optionsColorBoolFalse)
                                baseStatusMessage = baseStatusMessage.replace("!val!", "Enabled" if varVal else "Disabled")
                            else:
                                baseStatusMessage = baseStatusMessage.replace("!colorCode!", self.optionsColorOther)
                                baseStatusMessage = baseStatusMessage.replace("!val!", "{}".format(varVal))

                            retMessage += baseStatusMessage
                else:
                    if(len(moduleVars) != 0):
                        retMessage += "\n..."

                if(len(retMessage) + len(retMessageLong) >= 1024):
                    self.sendTextMessage(schid, targetMode, retMessageLong, targetID)
                    retMessageLong = retMessage
                else:
                    retMessageLong += retMessage

                retMessage = ""

            self.sendTextMessage(schid, targetMode, retMessageLong, targetID)
            return

        elif(len(parameters) == 1):
            nameIn = parameters[0]

            retMessage = ""

            for moduleName, moduleList in self.optionsDict.items():
                if(moduleName.lower() == nameIn.lower()):
                    isEnabled = getattr(self, moduleList[0])
                    moduleVars = moduleList[1]
                    moduleExplanation = moduleList[2]

                    retMessage = "\n--- [[color={}]{}[/color]]".format(self.optionsColorBoolTrue if isEnabled else self.optionsColorBoolFalse, moduleName)

                    if(len(moduleVars) == 0):
                        retMessage += "\n{}\n\nNo module vars for this module.".format(moduleExplanation)
                        self.sendLongTextMessage(schid, targetMode, retMessage, targetID, "\n")
                        return

                    retMessage += "\n{}\n\nModule variables".format(moduleExplanation)

                    for varList in moduleVars:
                        varName, varExplanation = varList[0], varList[1]
                        varVal = getattr(self, varName)

                        retMessage += "\n - {}:".format(varName)
                        retMessage += "\n   -> {}".format(varExplanation)

                        if(len(varList) == 2):
                            if(type(varVal) == type(True)):
                                varShowVal = "[color={}]{}[/color]".format(self.optionsColorBoolTrue if varVal else self.optionsColorBoolFalse, "Enabled" if varVal else "Disabled")
                            else:
                                varShowVal = "[color={}]{}[/color]".format(self.optionsColorOther, varVal)
                        elif(len(varList) == 4):
                            varShowVal = varList[2]
                            varShowVal = varShowVal.replace("!var!", varName)
                            varShowVal = varShowVal.replace("!val!", varVal)

                        retMessage += "\n   -> Value: {}".format(varShowVal)

                    self.sendLongTextMessage(schid, targetMode, retMessage, targetID, "\n")
                    return

            self.sendTextMessage(schid, targetMode, "[color=#ff0000]The module '[B]{}[/B]' does not exist.[/color]".format(parameters[0]), targetID)
            return


        elif(len(parameters) == 2):
            nameIn, varVal = parameters[0], parameters[1]

            for moduleName, moduleList in self.optionsDict.items():
                moduleVars = moduleList[1]

                if(moduleName.lower() == nameIn.lower()):
                    if(varVal.lower() == "true" or varVal.lower() == "enable" or varVal.lower() == "enabled"):
                        setTo = True
                    elif(varVal.lower() == "false" or varVal.lower() == "disable" or varVal.lower() == "disabled"):
                        setTo = False
                    else:
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]This module operation does not exist. Use '{}options <module> <enable/disable>' to enable/disable a module[/color]", targetID)
                        return

                    currentlySetTo = getattr(self, moduleList[0])

                    if(currentlySetTo == setTo):
                        self.sendTextMessage(schid, targetMode, "The module [[color={}]{}[/color]] is already {}".format(self.optionsColorBoolTrue if currentlySetTo else self.optionsColorBoolFalse, moduleName, "enabled" if currentlySetTo else "disabled"), targetID)
                    else:
                        setattr(self, moduleList[0], setTo)
                        self.sendTextMessage(schid, targetMode, "The module [[color={}]{}[/color]] is now {}".format(self.optionsColorBoolTrue if setTo else self.optionsColorBoolFalse, moduleName, "enabled" if setTo else "disabled"), targetID)
                    return

        nameIn, varVal = parameters[0], " ".join(parameters[1:])

        for moduleName, moduleList in self.optionsDict.items():
            moduleVars = moduleList[1]

            for varList in moduleVars:
                if(varList[0].lower() == nameIn.lower()):
                    varParsed = self.parseVariableFromString(varVal)
                    currentValue = getattr(self, varList[0])

                    if(type(varParsed) != type(currentValue)):
                        self.sendTextMessage(schid, targetMode, "[color=#ff0000]The entered variables type '[B]{}[/B]' doesn't match the type of '[B]{}[/B]': [B]{}[/B][/color]".format(type(varParsed), varList[0], type(currentValue)), targetID)
                        return

                    setattr(self, varList[0], varParsed)

                    if(len(varList) == 4):
                        retMessage = varList[3]
                        retMessage = retMessage.replace("!var!", varList[0])
                        retMessage = retMessage.replace("!val!", "{}".format(varParsed))
                        self.sendTextMessage(schid, targetMode, retMessage, targetID)
                    else:
                        self.sendTextMessage(schid, targetMode, "[color=#00aa00]Successfully set [B]{}[/B] to value '[B]{}[/B]'[/color]".format(varList[0], varParsed), targetID)
                    return

        self.sendTextMessage(schid, targetMode, "[color=#ff0000]The variable '[B]{}[/B]' was not found.[/color]".format(parameters[0]), targetID)




#-----------------------------------------------------------
#-----------------TOKEN TIMERS------------------------------

    def onClientDBIDfromUIDEvent(self, schid, clientUID, clientDBID):
        #self.debug("reached onClientDBIDfromUIDEvent")
        if(self.tokenOperationParams == [] or self.tokenOperationParams[0] != schid):
            return
        tokenName, tokenTargetID, targetName, senderName = self.tokenOperationParams[1], self.tokenOperationParams[2], self.tokenOperationParams[3], self.tokenOperationParams[4]
        self.tokenOperationParams = []


        if(tokenName.startswith("gag")):
            useGroupID = self.tokenGagGroupID
            messageToTarget = "[B]'{}'[/B] used a gag token lasting [B]'{}'[/B] on you".format(senderName, self.tokenListTokens[tokenName][2])
        elif(tokenName.startswith("sticky")):
            useGroupID = self.tokenStickyGroupID
            messageToTarget = "[B]'{}'[/B] used a sticky token lasting [B]'{}'[/B] on you".format(senderName, self.tokenListTokens[tokenName][2])
            self.tokenStickyClientChannels[clientUID] = ts3lib.getChannelOfClient(schid, self.getClientIDByName(schid, self.getClientNameByUID(schid, clientUID)))[1]
        elif(tokenName.startswith("mute")):
            useGroupID = self.tokenMuteGroupID
            messageToTarget = "[B]'{}'[/B] used a mute token lasting [B]'{}'[/B] on you".format(senderName, self.tokenListTokens[tokenName][2])
        else:
            self.debug("??? (ErrorCode: 69)")
            return

        ts3lib.requestServerGroupAddClient(schid, useGroupID, clientDBID)
        times = self.getTimesFromTimeString(self.tokenListTokens[tokenName][2])
        self.timedCall(self.removeTokenEffect, [schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, tokenTargetID, targetName], [tokenName, clientDBID, senderName], times=times, timerStorage=self.tokenTimers)
        self.sendTextMessage(schid, ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, messageToTarget, tokenTargetID)


    def onServerGroupClientListEvent(self, schid, serverGroupID, clientDBID, clientName, clientUID):#tokenUnstuck = [schid, targetMode, targetID, senderName]
        if(self.tokenUnstuck[0] != schid): return
        if(self.tokenUnstuck[3] != clientName): return
        ts3lib.requestServerGroupDelClient(schid, serverGroupID, clientDBID)

    def removeTokenEffect(self, schid, targetMode, targetID, targetName, parameters):#parameters = [tokenName, clientDBID, invokerName]
        #self.debug("Got to removeTokenEffect:\nparams: '{}', targetID '{}', targetName '{}'".format(parameters, targetID, targetName))
        if(self.getClientIDByName(schid, targetName) == -1):
            clientIsOffline = True

        tokenName, clientDBID, invokerName = parameters[0], parameters[1], parameters[2]

        if(tokenName.startswith("gag")):
            ts3lib.requestServerGroupDelClient(schid, self.tokenGagGroupID, clientDBID)
        elif(tokenName.startswith("sticky")):
            ts3lib.requestServerGroupDelClient(schid, self.tokenStickyGroupID, clientDBID)
            del(self.tokenStickyClientChannels[self.getClientUIDByName(schid, targetName)])
        if(tokenName.startswith("mute")):
            ts3lib.requestServerGroupDelClient(schid, self.tokenMuteGroupID, clientDBID)


    def onUpdateClientEvent(self, schid, clientID, invokerID, invokerName, invokerUID):
        clientUID = self.getClientUIDByID(schid, clientID)
        if(not clientUID in self.tokenStickyClientChannels):
            return

        channelID = self.tokenStickyClientChannels[clientUID]

        if((ts3lib.getChannelOfClient(schid, clientID)[1] != channelID) and self.clientHasExistingTokenEffect(schid, clientUID)):
            ts3lib.requestClientMove(schid, clientID, channelID, "")


    def textShot(self, schid, targetMode, targetID, senderName, parameters):#parameters = [listMessagesAndDelays, i]
        listMessagesAndDelays, i = parameters[0], parameters[1]
        self.sendTextMessage(schid, targetMode, listMessagesAndDelays[i][0], targetID)
        if(i < len(listMessagesAndDelays)-1):
            self.timedCall(self.textShot, [schid, targetMode, targetID, senderName], [listMessagesAndDelays, i+1], times=[0, listMessagesAndDelays[i+1][1], 0, 0, 0, 0], timerStorage=self.textTimers)


    def pokeShot(self, schid, targetMode, targetID, senderName, parameters):#parameters = [tokenName, targetName, amount, waitInSeconds, i]
        tokenName, targetName, amount, waitInSeconds, i = parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]

        targetClientID = self.getClientIDByName(schid, targetName)
        if(targetClientID == -1):
            factorDone = (i-1) / amount
            factorLeft = 1 - factorDone
            compensation = factorLeft * self.tokenListTokens[tokenName][1]

            finalComp = int(round(compensation, 0))
            self.sendTextMessage(schid, targetMode, "[color=#ff0000]Target of the poke is no longer online. The sender has been awarded a compensation of [B]'{} Pts'[/B][/color]".format(finalComp), targetID)

            senderUID = self.getClientUIDByName(schid, senderName)
            self.ecoChangeBalance(senderUID, finalComp)
            return

        ts3lib.requestClientPoke(schid, targetClientID, "Annoying poke ({}) by '{}'".format(i+1, senderName))

        if(i < amount-1):
            self.timedCall(self.pokeShot, [schid, targetMode, targetID, senderName], [tokenName, targetName, amount, waitInSeconds, i+1], times=[0, waitInSeconds, 0, 0, 0, 0], timerStorage=self.pokeTimers)

    def clientHasExistingTokenEffect(self, schid, clientUID):
        for timer in self.tokenTimers:
                listParams = self.tokenTimers[timer]
                timerClientUID = listParams[1][3]

                if(timerClientUID == clientUID):
                    return True

        return False

#-----------------------------------------------------------
#------------------------BETTING----------------------------
#betOpenTimers = {}#{timer : method, [schid, targetMode, targetID, senderName], [senderUID, targetUID, winAmount]}
#betOngoingTimers = {}#{timer : method, [schid, targetMode, targetID, senderName], [senderUID, targetUID, winAmount, correctNum, senderNum, targetNum]}

    def betHasRequestedBet(self, senderUID):
        for timer in self.betOpenTimers:
            timerSenderUID, timerTargetUID = self.betOpenTimers[timer][2][0], self.betOpenTimers[timer][2][1]
            if(senderUID == timerSenderUID):
                return True
        return False

    def betHasBetRequest(self, senderUID):
        for timer in self.betOpenTimers:
            timerSenderUID, timerTargetUID = self.betOpenTimers[timer][2][0], self.betOpenTimers[timer][2][1]
            if(senderUID == timerTargetUID):
                return True
        return False

    def betGetOpenBetWinAmount(self, senderUID):
        for timer in self.betOpenTimers:
            timerSenderUID, timerTargetUID = self.betOpenTimers[timer][2][0], self.betOpenTimers[timer][2][1]
            if(senderUID == timerTargetUID):
                return self.betOpenTimers[timer][2][2]
        return -1

    def betHasOngoingBet(self, senderUID):
        for timer in self.betOngoingTimers:
            timerSenderUID, timerTargetUID = self.betOngoingTimers[timer][2][0], self.betOngoingTimers[timer][2][1]
            if(senderUID == timerSenderUID or senderUID == timerTargetUID):
                return True
        return False

    def betStopBet(self, anyUID, isSender):
        for timer in self.betOpenTimers:
            timerSenderUID, timerTargetUID, winAmount = self.betOpenTimers[timer][2][0], self.betOpenTimers[timer][2][1], self.betOpenTimers[timer][2][2]
            if(isSender == True and anyUID == timerSenderUID):
                del(self.betOpenTimers[timer])
                timer.stop()
                self.ecoChangeBalance(timerSenderUID, winAmount)
                return 0

            elif(isSender == False and anyUID == timerTargetUID):
                del(self.betOpenTimers[timer])
                timer.stop()
                self.ecoChangeBalance(timerSenderUID, winAmount)
                return 0

    def betOpenBetTimedOut(self, schid, targetMode, targetID, senderName, parameters):
        senderUID, targetUID, winAmount = parameters[0], parameters[1], parameters[2]
        self.sendTextMessage(schid, targetMode, "[color=#ff0000]Your opponent didn't answer in time, [B]'{}'[/B].[/color]".format(senderName), targetID)
        self.ecoChangeBalance(senderUID, winAmount)

    def betStartOpenBet(self, cmdParameters, senderUID, targetUID, winAmount):
        times = self.getTimesFromTimeString("1m")
        self.timedCall(self.betOpenBetTimedOut, cmdParameters, [senderUID, targetUID, winAmount], times, timerStorage=self.betOpenTimers, ignoreOfflineCheck=True)
        self.ecoChangeBalance(senderUID, winAmount * -1)
        return 0



    def betOngoingBetTimedOut(self, schid, targetMode, targetID, senderName, parameters):
        senderUID, targetUID, winAmount, winningNumber, senderNum, targetNum = parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]
        if(senderNum == -1 and targetNum != -1):
            winnerUID = targetUID
            loserUID = senderUID
        elif(senderNum != -1 and targetNum == -1):
            winnerUID = senderUID
            loserUID = targetUID
        elif(senderNum == -1 and targetNum == -1):
            winnerUID = None


        if(winnerUID == None):
            self.sendTextMessage(schid, targetMode, "The bet timed out! Since neither of the contestants entered a number, neither of them wins the [B]{} Pt(s)[/B]. (TriHard my points now)".format(winAmount*2), targetID)
        else:
            winnerName = self.getClientNameByUID(schid, winnerUID)
            loserName = self.getClientNameByUID(schid, loserUID)
            self.ecoChangeBalance(winnerUID, winAmount * 2)
            self.sendTextMessage(schid, targetMode, "Since [B]'{}'[/B] didn't enter a number, [B]'{}'[/B] wins the bet and earns [B]{} Pt(s)[/B]. (The winning number was [B]{}[/B] btw)".format(loserName, winnerName, winAmount * 2, winningNumber), targetID)

    def betEnterNumber(self, anyUID, number):
        for timer in self.betOngoingTimers:
            schid, targetMode, targetID = self.betOngoingTimers[timer][1][0], self.betOngoingTimers[timer][1][1], self.betOngoingTimers[timer][1][2]
            senderUID, targetUID, winAmount = self.betOngoingTimers[timer][2][0], self.betOngoingTimers[timer][2][1], self.betOngoingTimers[timer][2][2]
            winningNumber, senderNum, targetNum = self.betOngoingTimers[timer][2][3], self.betOngoingTimers[timer][2][4], self.betOngoingTimers[timer][2][5]

            if(anyUID == senderUID):
                if(senderNum != -1):
                    return -1       #Already has a number entered
                self.betOngoingTimers[timer][2][4] = number
                senderNum = number

            elif(anyUID == targetUID):
                if(targetNum != -1):
                    return -1       #Already has a number entered
                self.betOngoingTimers[timer][2][5] = number
                targetNum = number

            if(senderNum == -1 or targetNum == -1):
                return 0

            del(self.betOngoingTimers[timer])
            timer.stop()

            diffSender = self.max(winningNumber, senderNum) - self.min(winningNumber, senderNum)
            diffTarget = self.max(winningNumber, targetNum) - self.min(winningNumber, targetNum)

            if(diffSender < diffTarget):
                winnerUID = senderUID
                winnerNum = senderNum
                loserUID = targetUID
                loserNum = targetNum
            elif(diffSender > diffTarget):
                winnerUID = targetUID
                winnerNum = targetNum
                loserUID = senderUID
                loserNum = senderNum
            else:
                #exploitable: both always bet the same number, they either get their cash back or 4x their cash back.
                if(diffSender == 0):
                    self.ecoChangeBalance(senderUID, winAmount * 4)
                    self.ecoChangeBalance(targetUID, winAmount * 4)
                    self.sendTextMessage(schid, targetMode, "Woah, you both had the exact number [B]{}[/B]? I call hax... You [B]both[/B] win the twice the amount: [B]{} Pt(s)[/B]!".format(winningNumber, winAmount*4), targetID)
                elif(senderNum != targetNum):
                    self.ecoChangeBalance(senderUID, winAmount * 2)
                    self.ecoChangeBalance(targetUID, winAmount * 2)
                    self.sendTextMessage(schid, targetMode, "Woah, you both had the same difference to the correct number [B]{}[/B]. You [B]both[/B] win the [B]{} Pt(s)[/B]!".format(winningNumber, winAmount*2), targetID)
                else:
                    self.ecoChangeBalance(senderUID, winAmount)
                    self.ecoChangeBalance(targetUID, winAmount)
                    self.sendTextMessage(schid, targetMode, "You both entered the same number, so you both get your cash back. You both got [B]{} Pt(s)[/B] back.".format(winAmount), targetID)
                return


            winnerName = self.getClientNameByUID(schid, winnerUID)
            loserName = self.getClientNameByUID(schid, loserUID)

            self.ecoChangeBalance(winnerUID, winAmount * 2)
            self.sendTextMessage(schid, targetMode, "[B]{}{} {}[/B] was closer to the correct number [B][color=#0000aa]{}[/color][/B] than [B]{}{} {}[/B] and won [B]{} Pt(s)[/B]".format(winnerName, "'s" if not winnerName.endswith("s") else "'", winnerNum, winningNumber, loserName, "'s" if not loserName.endswith("s") else "'", loserNum, winAmount*2), targetID)
            return 1



    def betStartOngoingBet(self, cmdParameters, targetUID):
        found = False
        for timer in self.betOpenTimers:
            timerTargetUID = self.betOpenTimers[timer][2][1]
            if(targetUID == timerTargetUID):

                senderUID = self.betOpenTimers[timer][2][0]
                winAmount = self.betOpenTimers[timer][2][2]

                del(self.betOpenTimers[timer])
                timer.stop()
                found = True
                break

        if(found == False):
            return -1       #target does not have a bet request

        self.ecoChangeBalance(targetUID, winAmount * -1)
        times = self.getTimesFromTimeString("2m")
        winningNumber = randint(1, 100)
        self.timedCall(self.betOngoingBetTimedOut, cmdParameters, [senderUID, targetUID, winAmount, winningNumber, -1, -1], times, timerStorage=self.betOngoingTimers, ignoreOfflineCheck=True)


#(self, method, cmdParameters, parameters, times, timerStorage=None, ignoreOfflineCheck=False)
#-----------------------------------------------------------
#-----------------------OPTIONS-----------------------------

    def prepareOptionsVar(self):
        self.optionsDict = collections.OrderedDict()

        #Commands should be:
        #!options -> list of all modules and, if they are enabled, all variables in the modules
        #!options <moduleName> -> explanation of the module and all variables in the module
        #!options <varName> <value...> -> set a variable to a given value (disallow setting different type for variable!)
        #
        #default is ["variableInModule", "explanation", "variableName -> variableValue", "'variableName' set to 'newVariableValue'"]
        #           |______________________________________________________________________________________________________________|
        #                                                                          |
        #                                                     _____________________|_____________________
        #                                                    |                                           |
        #self.optionsDict["moduleName"] = ["enableVarName", [["optionsVar1Name", "optionsVar1Explanation"]], "moduleExplanation"]

        self.optionsDict["ChatCommands"] = ["chatCommandsEnabled",
                                            [["chatCommandPrefix", "The prefix for messages to be recognized as chat commands"]],
                                            "All chat commands are managed through this module."]


        self.optionsDict["Debug"] = ["debugPlugin",
                                     [],
                                     "Debug this plugin. Disable this to not recieve any kind of warnings in the chat."]


        self.optionsDict["LinkReaction"] = ["immediateLinkReaction",
                                            [],
                                            "Enable or disable the automatic information gathering for youtube/twitch links."]


        self.optionsDict["RevengePoke"] = ["revengePokeEnabled",
                                           [["revengePokeMessage", "The message the poker will get when he is poked"]],
                                           "A revenge-poke system. Anyone that pokes you will get poked back and you won't be notified of their poke!"]


        self.optionsDict["Door"] = ["doorEnabled",
                                    [["doorKickMessage", "The message entered in the server-kick when someone enters the door channel"],
                                     ["doorChannelName", "The name of the channel that is recognized as the door channel"]],
                                    "If you enter the door-channel, you get kicked. Obviously you need server kick permissions for this."]


        self.optionsDict["Highlighting"] = ["highlightEnabled",
                                            [],
                                            "Highlighting system for chat. Whenever someone excluding yourself writes a keyphrase, you will be notified in a chat with yourself.\nType '{}help highlight' for more information on the highlight module".format(self.chatCommandPrefix)]


        self.optionsDict["AFK"] = ["afkEnabled",
                                   [["afkPrefix", "The prefix in your name to be recognized as AFK"],
                                    ["afkMessage", "The message anyone that private messages or pokes you while AFK"],
                                    ["afkBotTag", "The prefix of the response message to prevent the bot from answering itself"]],
                                   "AFK system to notify others that you are away. Set the prefix at the start of your name to use this"]


        self.optionsDict["Economy"] = ["ecoEnabled",
                                       [["ecoTickTimeInSeconds", "The time (in seconds) it takes for everyone to recieve 1 point"],
                                        ["ecoEnabledForServerUID", "The server to use for gaining points"],
                                        ["ecoPointLimit", "The maximum amount of points one can have"]],
                                       "Point based economy system used for various other modules, such as Tokens and TTS"]


        self.optionsDict["Betting"] = ["betEnabled",
                                        [["betMinAmount", "The minimum amount of points to bet"],
                                         ["betMaxAmount", "The maximum amount of points to bet"]],
                                        "A betting system to challenge someone else to a duel."]


        self.optionsDict["Roulette"] = ["roulEnabled",
                                        [["roulWinChancePercent", "The chance (in percent) to win the gamble"],
                                         ["roulWinMultiplier", "When you win the gamble, you recieve this number times the input as balance back"],
                                         ["roulCooldown", "The cooldown for the '{}roulette' command to prevent spamming".format(self.chatCommandPrefix)],
                                         ["roulSpecialWinSound", "The sound to be played when someone hits the jackpot"]],
                                        "Roulette system for making easy money (or losing easy money)."]


        self.optionsDict["AutoReconnect"] = ["rcEnabled",
                                             [["rcRevengeKickEnabled", "Whether or not you want your kicker to be kicked. (You need kick privileges for this one, obviously)"],
                                              ["rcRevengeKickMessage", "The message to return to your kicker. Use the placeholder '!kickmsg!' for the reason your kicker kicked you"],
                                              ["rcDelay", "The delay (in seconds) between being kicked and trying to reconnect to the server"]],
                                             "Automatic reconnect system when being kicked. Great for annoying admins (and provoking perm bans)!"]


        self.optionsDict["AntiCorruption"] = ["acEnabled",
                                              [["acAdminGroupID", "The ID of the admin group. This is to identify when your group has been taken away"],
                                               ["acPrivKey", "The privilege key used to restore your server group. WARNING: Create and set a new key after it was used once!", "[color=#9933dd]~hidden~[/color]", "[color=#00aa00]Successfully set [B]!var![/B] to [color=#9933dd]~hidden~[/color]"]],
                                              "Anti corruption system against trolls/whatever. When your server group is taken away you automatically use the privilege key and kick the perpetrator"]


        self.optionsDict["Tokens"] = ["tokenEnabled",
                                      [["tokenGagGroupID", "The ID of the gag-group"],
                                       ["tokenStickyGroupID", "The ID of the sticky-group"],
                                       ["tokenMuteGroupID", "The ID of the mute-group"]],
                                      "A token system utilizing the eco system. If enabled, users can spend points with the '{}token' command for the desired effect".format(self.chatCommandPrefix)]


        self.optionsDict["Playsound"] = ["psEnabled",
                                         [["psDefaultVolume", "The default playback volume"],
                                          ["psDefaultSpeed", "The default playback speed"]],
                                         "Use Playsound to let users buy sounds via points and play them through the hosts mic. The sounds can be modified with the options '-earrape', '-nc' and '-demon'"]


        self.optionsDict["Daily"] = ["dailyEnabled",
                                     [["dailyReward", "The amount of points the daily reward should be"],
                                     ["dailyCooldown", "The amount of cooldown the reward should have"]],
                                     "Users can earn a little bonus by using this command daily!"]


        self.optionsDict["NowPlaying"] = ["npEnabled",
                                          [["npSCFilePath", "The path to the StreamCompanion folder that holds the auto updated now playing files on your filesystem, e.g. 'C:\\Program Files (x86)\\StreamCompanion\\Files'"]],
                                          "NowPlaying provides osu! integration for Teamspeak. When you have StreamCompanion running you can print its information directly to chat using the '{}np' commands".format(self.chatCommandPrefix)]


        self.optionsDict["TTS"] = ["ttsEnabled",
                                   [["ttsVolume", "The volume of the sound being played, default is '1.0'"],
                                   ["ttsCharsPerPoint", "The amount of characters (spaces not included) one can buy with 1 point. Always atleast 1 point."],
                                   ["ttsCDPerChar", "The cooldown (in seconds) of the command per character written"],
                                   ["ttsMinCD", "The minimum amount of cooldown (in seconds) for each command"],
                                   ["ttsMaxCD", "The maximum amount of cooldown (in seconds) for each command"],
                                   ["ttsMaxChars", "The maximum amount of characters allowed in one TTS message"],
                                   ["ttsUnicodeIncrease", "Higher price for larger unicode characters such as emojies"],
                                   ["ttsGlobalOutput", "Your audio device used for global output"]],
                                   "A text-to-speech system that allows users to let text be read out through your audio output in various languages and translations. See the documentation on how to setup TTS".format(self.chatCommandPrefix)]


        self.optionsDict["QuickChat"] = ["quickChatEnabled",
                                         [["quickChat1", "Quick-Chat slot 1"],
                                          ["quickChat2", "Quick-Chat slot 2"],
                                          ["quickChat3", "Quick-Chat slot 3"]],
                                         "A Quick Chat system that can be used through Teamspeaks hotkeys."]


        self.optionsDict["FloodPrevention"] = ["fpEnabled",
                                               [["fpMaxPokesPerMinute", "Maximum number of pokes you recieve per minute from one client"],
                                               ["fpMaxPrivatePerMinute", "Maximum number of private messages you recieve per minute from one client"],
                                               ["fpMaxCommandsPerMinute", "Maximum number of commands one client can issue per minute"]],
                                               "FloodPrevention suppresses excess actions sent by other clients"]

#-----------------------------------------------------------
#-----------------------DAILIES-----------------------------

    def dailyGetTimeLeft(self, senderUID):
        file = self.getFile(self.getPathToFile(self.dailySaveFile), "r")

        for line in file:
            line = line.rstrip()

            (clientUID, timeString) = line.split(";")

            if(clientUID == senderUID):
                cooldownDate = datetime.strptime(timeString, "%Y-%m-%d %H:%M:%S.%f")
                now = datetime.now()

                if(cooldownDate > now):
                    file.close()
                    delta = cooldownDate - now
                    return self.formatSecondsToTimeString(int(delta.total_seconds()), mode=1)
                break

        file.close()

        return None


    def dailySetOnCooldown(self, senderUID):
        file = self.getFile(self.getPathToFile(self.dailySaveFile), "r")

        allLines = []

        for line in file:
            (clientUID, timeString) = line.split(";")
            if(clientUID != senderUID):
                allLines.append(line.rstrip())

        file.close()

        file = self.getFile(self.getPathToFile(self.dailySaveFile), "w")

        for line in allLines:
            file.write("{}\n".format(line))


        now = datetime.now()
        if(self.dailyUseCooldownAsResetTime == True):
            hours, minutes = int(self.dailyCooldown.split(":")[0]), int(self.dailyCooldown.split(":")[1])

            atTime = datetime.strptime("{} {:02}:{:02}:00.000000".format(now.strftime("%Y-%m-%d"), hours, minutes), "%Y-%m-%d %H:%M:%S.%f")

            if(now.hour > hours or (now.hour == hours and now.minute > minutes)):
                cooldownDate = atTime + timedelta(days=1)
            else:
                cooldownDate = atTime


            file.write("{};{}.000000\n".format(senderUID, cooldownDate))
        else:
            self.debug("2")
            times = self.getTimesFromTimeString(self.dailyCooldown)
            cooldownDate = now + timedelta(milliseconds=times[0], seconds=times[1], minutes=times[2], hours=times[3], days=times[4], weeks=times[5])

            file.write("{};{}\n".format(senderUID, cooldownDate))

        file.close()
        return True


    #Y: 2018, M: 12, D: 31, H: 23, M: 59, S: 59
    #Y: 2019, M: 00, D: 00, H: 00, M: 00, S: 00



#-----------------------------------------------------------
#-----------------------REWARDS-----------------------------

    def useCode(self, code, senderUID, senderName):
        codeInfo = self.getCodeInfo(code)

        if(codeInfo == None):
            return (-1,)

        codeType, codeDate, codeReward, args = codeInfo[1], codeInfo[2], codeInfo[3], codeInfo[4]

        now = datetime.now()

        if(now > codeDate):
            return (-2, codeInfo)

        tooManyPoints = False
        if(self.ecoGetBalance(senderUID) + codeReward > self.ecoPointLimit * 2):
            tooManyPoints = True

        if(codeType == "single-use"):
            if(codeInfo[5] != "-"):
                return (-4, codeInfo)
            else:
                writeLine = [code, codeType, codeDate, codeReward, " ".join(args), senderUID, senderName]

        elif(codeType == "unlimited-use"):
            usedByUIDs = codeInfo[5]

            if(usedByUIDs[0] == "-"):
                writeLine = [code, codeType, codeDate, codeReward, " ".join(args), senderUID]
            else:
                for uid in usedByUIDs:
                    if(senderUID == uid):
                        return (-5, codeInfo)
                writeLine = [code, codeType, codeDate, codeReward, " ".join(args), *usedByUIDs, senderUID]

        if(tooManyPoints == True):
            return (-3, codeInfo)

        self.ecoChangeBalance(senderUID, codeReward)
        retMessage = (0, codeInfo)


        if(len(writeLine) != 0):
            file = self.getFile(self.rewardsSaveFile, "r")

            lines = []
            for line in file:
                lines.append(line.rstrip())

            file.close()

            file = self.getFile(self.rewardsSaveFile, "w")

            for line in lines:
                code = line.split(";")[0]
                if(code == writeLine[0]):
                    file.write("{}\n".format(";".join([str(x) for x in writeLine])))
                else:
                    file.write("{}\n".format(line))
            file.close()

            return retMessage
        else:
            self.debug("useCode(...): no line to write was found...")


    def getCodeInfo(self, code):
        file = self.getFile(self.rewardsSaveFile, "r")

        for line in file:
            line = line.rstrip()

            params = line.split(";")

            if(params[0] == code):
                file.close()
                tempType, tempExpiredDateStr, tempRewardStr, argsStr = params[1], params[2], params[3], params[4]

                if(tempType == "single-use"):   #code;single-use;date;points;argsStr;usedByUID;usedByName
                    usedByUID, usedByName = params[5], params[6]
                    return (code, tempType, datetime.strptime(tempExpiredDateStr, "%Y-%m-%d %H:%M:%S.%f"), int(tempRewardStr), argsStr.split(" "), usedByUID, usedByName)

                elif(tempType == "unlimited-use"):  #code;unlimited-use;date;points;argsStr;uid1;uid2;uid3;uid4;...
                    usedByUIDs = params[5:]
                    return (code, tempType, datetime.strptime(tempExpiredDateStr, "%Y-%m-%d %H:%M:%S.%f"), int(tempRewardStr), argsStr.split(" "), usedByUIDs)

        return None


    def createCode(self, type, times, rewardAmount, argsStr):
        if(type.lower() == "single" or type.lower() == "single-use" or type.lower() == "single_use"):
            type = "single-use"
        elif(type.lower() == "unlimited" or type.lower() == "unlimited-use" or type.lower() == "unlimited_use"):
            type = "unlimited-use"


        now = datetime.now()
        expireDate = now + timedelta(milliseconds=times[0], seconds=times[1], minutes=times[2], hours=times[3], days=times[4], weeks=times[5])

        done = False

        while(done == False):
            alph = string.ascii_uppercase + string.digits
            all = list(choice(alph) for _ in range(16))
            randomLet = "".join(all)
            rewardCode = "{}-{}-{}-{}".format(randomLet[0:4], randomLet[4:8], randomLet[8:12], randomLet[12:16])

            done = True

            file = self.getFile(self.rewardsSaveFile, "r")

            lines = []
            for line in file:
                lines.append(line.rstrip())

            file.close()

            file = self.getFile(self.rewardsSaveFile, "w")

            for line in lines:
                tempCode = line.split(";")[0]
                if(tempCode == rewardCode):
                    done = False
                file.write("{}\n".format(line))


        if(type == "single-use"):                   #code;single-use;date;points;usedByUID;usedByName
            file.write("{};{};{};{};{};-;-".format(rewardCode, type, expireDate, rewardAmount, argsStr))
        elif(type == "unlimited-use"):
            file.write("{};{};{};{};{};-".format(rewardCode, type, expireDate, rewardAmount, argsStr))

        file.close()

        return rewardCode

#-----------------------------------------------------------
#---------------------PLAYSOUNDS----------------------------

    def loadStats(self):
        if(os.path.exists(self.getPathToFile("stats.json"))):
            file = self.getFile("stats.json", "r")
            self.statsData = json.loads(file.read())
            file.close()
        else:
            self.statsData = {}

    def saveStats(self):
        file = self.getFile("stats.json", "w")
        json.dump(self.statsData, file, sort_keys=True)
        file.close()


    def statsModuleExists(self, module):
        if(not module in self.statsData):
            return False
        return True

    def statsGet(self, module, name):
        if(not module in self.statsData or not name in self.statsData[module]):
            return False

        return self.statsData[module][name][0]

    def statsSet(self, module, name, value, order):
        if(not module in self.statsData):
            self.statsData[module] = {}

        self.statsData[module][name] = (value, order)

    def statsIncrement(self, module, name, step, order):
        ret = self.statsGet(module, name)
        if(ret != False):
            step += ret
        self.statsSet(module, name, step, order)



#-----------------------------------------------------------
#---------------------PLAYSOUNDS----------------------------

    def loadPlaysounds(self):
        if(not os.path.isdir(self.getPathToFile("playsounds"))):
            os.mkdir(self.getPathToFile("playsounds"))

        file = self.getFile(self.psSoundsFileName, "r")
        self.psSounds = collections.OrderedDict()

        for line in file:
            line = line.rstrip()
            split = line.split(";")

            if(len(split) != 4):
                self.debug("Error while loading playsounds: Invalid line in 'playsounds.txt': \"{}\"".format(line))
                continue

            try:
                cost = int(split[0])
            except:
                self.debug("Error while loading playsounds: Invalid price for sound: \"{}\"".format(line))
                continue

            soundFileName = split[1]
            soundFileWav = soundFileName+".wav"

            if(not os.path.isfile(self.getPathToFile("playsounds")+"\\"+soundFileWav)):
                self.debug("Error while loading playsounds: Could not find file \"{}\"".format(soundFileWav))
                continue

            soundDisplayName = split[2]

            soundSource = split[3]

            self.psSounds[soundDisplayName] = [soundFileName, cost, soundSource]


        self.psSounds = collections.OrderedDict(sorted(self.psSounds.items(), key = lambda x: x[1][1]))

        file.close()

        self.debug("Done loading playsounds. Loaded a total of [color=#00aa00]{}[/color] sounds.".format(len(self.psSounds)))
        #self.debug("psSounds -> {}".format(self.psSounds))

    def savePlaysounds(self):
        file = self.getFile(self.psSoundsFileName, "w")

        for soundDisplayName in self.psSounds:
            soundFileName = self.psSounds[soundDisplayName][0]
            cost = self.psSounds[soundDisplayName][1]
            soundSource = self.psSounds[soundDisplayName][2]

            file.write("{};{};{};{}\n".format(cost, soundFileName, soundDisplayName, soundSource))

        file.close()
        #self.debug("Done saving playsounds.")


    def play_wav(self, filepath, playbackSpeed=1, playbackVolume=1):
        #self.debug("params: '{}'".format(["pythonw", self.getPathToFile("playsound.pyw"), "device="+format(self.ttsGlobalOutput), "speed={}".format(playbackSpeed), "vol={}".format(playbackVolume), "path="+filepath]))
        self.psProcessStorage = subprocess.Popen(["pythonw", self.getPathToFile("playsound.pyw"), "device="+format(self.ttsGlobalOutput), "speed={}".format(playbackSpeed), "vol={}".format(playbackVolume), "path="+filepath])


    def convertMp3ToWav(self, filepath):
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startupinfo.wShowWindow = 0

        outputFilePath = ".".join(filepath.split(".")[0:-1])+".wav"

        self.debug("filepath in: {}".format(filepath))
        self.debug("filepath out: {}".format(outputFilePath))

        self.psProcessStorage = subprocess.Popen(["ffmpeg", "-y", "-loglevel", "quiet", "-i",  "{}".format(filepath), "-acodec", "pcm_s16le", "-ac", "1", "-ar", "16000", "{}".format(outputFilePath)], startupinfo=startupinfo)




#-----------------------------------------------------------
#-------------------FLOOD PREAVENTION-----------------------


    #fpActions = {actionType : {clientUID : [timePerformed1, timePerformed2, ...]}}
    #fpSupressed = {actionType : {clientUID : int}}

    def floodPrevent(self, senderUID, action):
        actionList = self.fpActions[action]
        if(not senderUID in actionList):
            actionList[senderUID] = []

        timesList = actionList[senderUID]

        if(action == "poke"):
            maxActionsPerMinute = self.fpMaxPokesPerMinute
        elif(action == "private_chat"):
            maxActionsPerMinute = self.fpMaxPrivatePerMinute
        elif(action == "command"):
            maxActionsPerMinute = self.fpMaxCommandsPerMinute

        if(maxActionsPerMinute == -1):
            return 0

        count = 0
        now = datetime.now()
        back60Seconds = now + timedelta(minutes=-1)

        for time in timesList:
            if(time > back60Seconds):
                count += 1
            else:
                timesList.remove(time)

        if(count >= maxActionsPerMinute):
            if(not senderUID in self.fpSupressed[action]):
                self.fpSupressed[action][senderUID] = 0
            self.fpSupressed[action][senderUID] += 1
            return 1
        else:
            self.fpActions[action][senderUID].append(now)
            return 0




#-----------------------------------------------------------
#------------------------LINKS------------------------------

    def linkReply(self, reply):
        pageContent = reply.readAll().data().decode("utf-8")
        self.debug("replyContent '{}'".format(pageContent))

        params = self.linkParams
        self.linkParams = {}

        schid = params["schid"]
        targetMode = params["targetMode"]
        targetID = params["targetID"]
        senderName = params["senderName"]
        action = params["action"]

        #self.debug("schid '{}' targetMode '{}' targetID '{}' senderName '{}' action '{}'".format(schid, targetMode, targetID, senderName, action))

        #if(not pageContent.startswith("OK")):
        #    self.sendTextMessage(schid, targetMode, "[color=#ff0000]The following error occured while parsing request: {}".format(pageContent[7:]), targetID)

        if(action == "link_request"):
            split = pageContent.split(";")

            longID = split[0]
            message = split[1]
            url = split[2]

            #self.debug("shortID '{}' longID '{}' message '{}' url '{}'".format(shortID, longID, message, url))

            self.sendTextMessage(schid, targetMode, "({}) ⇒ [url={}]{}[/url]".format(longID, url, message), targetID)

        elif(action == "link_add" or action == "link_remove"):
            if(pageContent == "OK"):
                self.sendTextMessage(schid, targetMode, "[color=#00aa00]Successfully {} the list[/color]".format("added to " if action == "link_add" else "removed from"), targetID)
            else:
                errorMessage = pageContent[7:]
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]{}[/color]".format(errorMessage), targetID)

        elif(action == "link_count"):
            if(pageContent.startswith("OK: ")):
                count = pageContent[4:]
                self.sendTextMessage(schid, targetMode, "The list currently contains [B]{}[/B] entries".format(count), targetID)
            else:
                errorMessage = pageContent[7:]
                self.sendTextMessage(schid, targetMode, "[color=#ff0000]{}[/color]".format(errorMessage), targetID)

#-----------------------------------------------------------
#------------------PRIVILEGE SYSTEM-------------------------

    def privSetClientLevel(self, clientUID, level):
        file = self.getFile(self.privFileName, "r")

        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            linePrivLevel = int(split[1])
            if(lineUID == clientUID):
                file.close()
                if(linePrivLevel != level):
                    self.privRemoveClient(clientUID)
                    break
                else:
                    return -1   #Client already has the asigned level

        if(file == None):
            file.close()

        file = self.getFile(self.privFileName, "a")
        file.write("{}:{}\n".format(clientUID, level))
        file.close()

        return 0

    def privRemoveClient(self, clientUID):
        file = self.getFile(self.privFileName, "r")

        listUsers = []
        found = False
        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            linePrivLevel = int(split[1])
            if(lineUID == clientUID and linePrivLevel != 0):
                found = True
            else:
                listUsers.append(line)
        file.close()

        if(found == False):
            return -1       #User has no asigned level or level of 0

        file = self.getFile(self.privFileName, "w")
        for line in listUsers:
            file.write(line+"\n")
        file.close()
        return 0


    def privGetClientLevel(self, clientUID):
        if(self.isMeByUID(clientUID)):
            #self.debug("its me '{}'".format(self.privMyLevel))
            return self.privMyLevel

        file = self.getFile(self.privFileName, "r")
        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            linePrivLevel = int(split[1])
            if(lineUID == clientUID):
                file.close()
                return linePrivLevel
        file.close()
        return 0
#-----------------------------------------------------------
#--------------------COOLDOWN-------------------------------

    #cooldownTimers = {timer123 : [clientUID, cdPhrase, notifyOnExpire, notifyMessage]}
    #times = [miliseconds, seconds, minutes, hours, days, weeks]

    def setCooldown(self, clientUID, cdPhrase, cooldownTimeString, notifyMessage=""):
        times = self.getTimesFromTimeString(cooldownTimeString)
        now = datetime.now()
        secondsDue = ((now.day * 24 + now.hour + times[3]) * 60 + now.minute + times[2]) * 60 + now.second + times[1]
        self.timedCall(self.onCooldownExpire, [ts3defines.TextMessageTargetMode.TextMessageTarget_CLIENT, clientUID], [clientUID, cdPhrase, notifyMessage, secondsDue], times=times, timerStorage=self.cooldownTimers, ignoreOfflineCheck=True)

    def onCooldownExpire(self, targetMode, clientUID, parameters):
        cdPhrase, notifyMessage = parameters[1], parameters[2]

        if(notifyMessage != ""):
            found = False
            for schid in ts3lib.getServerConnectionHandlerList()[1]:
                clientID = self.getClientIDByUID(schid, clientUID)
                if(clientID != -1):
                    self.sendTextMessage(schid, targetMode, notifyMessage, clientID)
                    found = True
                    break
            if(found == False):
                self.debug(" - onCooldownExpire: [color=#ff0000]Client supposed to be notified on '{}'-expire is no longer online[/color]".format(cdPhrase))

    def hasCooldown(self, clientUID, cdPhrase):#listParams[timer] = [method, cmdParameters, [clientUID, cdPhrase, notifyMessage], ignoreOfflineCheck]
        for timer in self.cooldownTimers:
            listParams = self.cooldownTimers[timer]
            parameters = listParams[2]
            timerClientUID, timerCooldownPhrase = parameters[0], parameters[1]
            if(clientUID == timerClientUID and cdPhrase == timerCooldownPhrase):
                return True
        return False

    def getRemainingCooldownAsString(self, clientUID, cdPhrase):
        for timer in self.cooldownTimers:
            listParams = self.cooldownTimers[timer]
            parameters = listParams[2]
            timerClientUID, timerCooldownPhrase, secondsDue = parameters[0], parameters[1], parameters[3]
            if(clientUID == timerClientUID and cdPhrase == timerCooldownPhrase):
                now = datetime.now()
                secondsNow = ((now.day * 24 + now.hour) * 60 + now.minute) * 60 + now.second

                secondsDiff = secondsDue - secondsNow

                timeString = self.formatSecondsToTimeString(secondsDiff, mode=1)

                return timeString

        return ""

#-----------------------------------------------------------
#----------------AUTO RECONNECT-----------------------------

    def reconnect(self, schid=None):
        schid = schid if schid else self.rcSchid

        if("port" in self.rcTabs[schid]):
            host = "{}:{}".format(self.rcTabs[schid]["host"], self.rcTabs[schid]["port"])# if hasattr(self.rcTabs[schid], 'port') else self.rcTabs[schid]["host"]
        else:
            host = "{}".format(self.rcTabs[schid]["host"])

        ts3lib.guiConnect(ts3defines.PluginConnectTab.PLUGIN_CONNECT_TAB_CURRENT, self.rcTabs[schid]["name"], host,
                          self.rcTabs[schid]["pw"], self.rcTabs[schid]["nick"], self.rcTabs[schid]["cpath"], self.rcTabs[schid]["cpw"], "", "", "", "", "", "", "")



    def saveTab(self, schid):
        if not hasattr(self.rcTabs, '%s'%schid):
            self.rcTabs[schid] = {}
        (err, self.rcTabs[schid]["name"]) = ts3lib.getServerVariable(schid, ts3defines.VirtualServerProperties.VIRTUALSERVER_NAME)
        (err, self.rcTabs[schid]["host"], self.rcTabs[schid]["port"], self.rcTabs[schid]["pw"]) = ts3lib.getServerConnectInfo(schid)
        (err, self.rcTabs[schid]["clid"]) = ts3lib.getClientID(schid)
        (err, self.rcTabs[schid]["nick"]) = ts3lib.getClientDisplayName(schid, self.rcTabs[schid]["clid"])
        (err, cid) = ts3lib.getChannelOfClient(schid, self.rcTabs[schid]["clid"])
        (err, self.rcTabs[schid]["cpath"], self.rcTabs[schid]["cpw"]) = ts3lib.getChannelConnectInfo(schid, cid)

#-----------------------------------------------------------
#-------------------GOOGLE API------------------------------
    def googleReply(self, reply):
        try:
            import json
            results = json.loads(reply.readAll().data().decode('utf-8'))["items"]
            for result in results:
                self.sendTextMessage(self.cmdevent["schid"], self.cmdevent["targetMode"], "[url={0}]{1}[/url]".format(result["link"],result["title"]), self.cmdevent["toID"])
            self.cmdevent = {"event": "", "returnCode": "", "schid": 0, "targetMode": 4, "toID": 0, "fromID": 0, "params": ""}
        except: from traceback import format_exc;ts3lib.logMessage(format_exc(), ts3defines.LogLevel.LogLevel_ERROR, "pyTSon", 0)

#-----------------------------------------------------------
#--------------------NO MOVE--------------------------------
    def addNoMove(self, clientUID):
        #self.debug("2. noMoveFileName '{}'".format(self.noMoveFileName))
        noMoveFile = self.getFile(self.noMoveFileName, "r")

        for zeile in noMoveFile:
            zeile = zeile.rstrip()
            if(clientUID == zeile):
                return -1   #Target UID is already on the list

        noMoveFile.close()
        noMoveFile = self.getFile(self.noMoveFileName, "a")
        noMoveFile.write(clientUID+"\n")
        noMoveFile.close()

        return 0

    def removeNoMove(self, clientUID):
        noMoveFile = self.getFile(self.noMoveFileName, "r")

        tmpList = []

        found = False

        for zeile in noMoveFile:
            zeile = zeile.rstrip()
            if(clientUID != zeile):
                tmpList.append(zeile)
            else:
                found = True

        noMoveFile.close()

        if(found == False):
            return -1

        noMoveFile = self.getFile(self.noMoveFileName, "w")

        for string in tmpList:
            noMoveFile.write("{}\n".format(string))

        noMoveFile.close()
        return 0


    def isNoMove(self, clientUID):
        noMoveFile = self.getFile(self.noMoveFileName, "r")

        for zeile in noMoveFile:
            zeile = zeile.rstrip()
            if(zeile == clientUID):
                noMoveFile.close()
                return True

        noMoveFile.close()
        return False
#-----------------------------------------------------------
#--------------------RevengePoke----------------------------

    def toggleRevengePoke(self):
        self.revengePokeEnabled = False if self.revengePokeEnabled == True else True
        ts3lib.printMessageToCurrentTab("RevengePoke has been {}".format("[color=#00aa00]enabled[/color]" if self.revengePokeEnabled == True else "[color=#ff0000]disabled"))

    def setRevengePokeMessage(self, message):
        self.revengePokeMessage = message




#-----------------------------------------------------------
#------------------------ECONOMY----------------------------

    def ecoChangeBalance(self, clientUID, amount=1):
        self.ecoBalances[clientUID] = self.ecoGetBalance(clientUID) + amount


    def ecoSetBalance(self, clientUID, amount):
        self.ecoBalances[clientUID] = amount

    def ecoGetBalance(self, clientUID):
        if(clientUID in self.ecoBalances):
            return self.ecoBalances[clientUID]
        else:
            self.ecoBalances[clientUID] = 0
            return 0

    def ecoCanPay(self, clientUID, price):
        if((self.ecoGetBalance(clientUID) < price) or self.ecoIsBlacklisted(clientUID) == True or price < 0):
            return False
        return True

    def ecoTransferBalance(self, fromUID, toUID, amount):
        if(not self.ecoCanPay(fromUID, amount)):
            return False

        self.ecoChangeBalance(fromUID, -amount)
        self.ecoChangeBalance(toUID, amount)
        return True

    def ecoSetExclusionValue(self, clientUID, type):   #type: 0 - Excluded, 1 - Blacklisted, -1 - Whitelisted
        file = self.getFile(self.ecoBlacklistFileName, "r")

        found = False

        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            lineExValue = split[1]
            if(lineUID == clientUID):
                found = True
                file.close()
                self.ecoRemoveExcluded(clientUID)
                break

        if(found == False):
            file.close()

        file = self.getFile(self.ecoBlacklistFileName, "a")
        file.write("{}:{}\n".format(clientUID, type))
        file.close()

        return 0

    def ecoRemoveExcluded(self, clientUID):
        file = self.getFile(self.ecoBlacklistFileName, "r")
        listUsers = []
        found = False

        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            lineExValue = split[1]
            if(lineUID == clientUID):
                found = True
            else:
                listUsers.append(line)
        file.close()

        if(found == False):
            return -1

        file = self.getFile(self.ecoBlacklistFileName, "w")
        for line in listUsers:
            file.write(line+"\n")
        file.close()
        return 0

    def ecoIsBlacklisted(self, clientUID):
        file = self.getFile(self.ecoBlacklistFileName, "r")
        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            lineExValue = split[1]
            if(lineUID == clientUID):
                file.close()
                if(lineExValue == str(self.ecoBlacklisted)):
                    return True
                return False
        file.close()
        return False

    def ecoIsWhitelisted(self, clientUID):
        file = self.getFile(self.ecoBlacklistFileName, "r")
        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            lineExValue = split[1]
            if(lineUID == clientUID):
                file.close()
                if(lineExValue == str(self.ecoWhitelisted)):
                    return True
                return False
        file.close()
        return False

    def ecoIsExcluded(self, clientUID):
        file = self.getFile(self.ecoBlacklistFileName, "r")
        for line in file:
            line = line.rstrip()
            if(line == ""): continue
            split = line.split(":")

            lineUID = split[0]
            lineExValue = split[1]
            if(lineUID == clientUID):
                file.close()
                if(lineExValue == str(self.ecoExcluded)):
                    return True
                return False
        file.close()
        return False

    def ecoTick(self):
        if(self.ecoEnabled == False):
            return

        (err, schList) = ts3lib.getServerConnectionHandlerList()

        performed = False

        for schid in schList:
            (err, serverUID) = ts3lib.getServerVariableAsString(schid, ts3defines.VirtualServerProperties.VIRTUALSERVER_UNIQUE_IDENTIFIER)
            #self.debug("serverUID '{}' enabledFor '{}'".format(serverUID, self.ecoEnabledForServerUID))
            if(serverUID == self.ecoEnabledForServerUID):
                performed = True
                (err, clientList) = ts3lib.getClientList(schid)
                for clientID in clientList:
                    (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
                    if(self.ecoIsBlacklisted(clientUID) == False and self.ecoIsExcluded(clientUID) == False and self.ecoGetBalance(clientUID) < self.ecoPointLimit):
                        self.ecoChangeBalance(clientUID, 1)

        if(performed == False):
            self.debug("Economy doesn't seem to be enabled for any currently connected server.")

    def ecoLoadFromFile(self):
        file = self.getFile(self.ecoFileName, "r")

        for line in file:
            line = line.rstrip()
            split = line.split(":")
            if(len(split) != 2):
                self.debug("checkEconomyFile(): found invalid format '{}', deleting row".format(line))
            else:
                if(len(split[0]) != 28):
                    self.debug("checkEconomyFile(): found invalid UID '{}', deleting row".format(split[0]))
                else:
                    try:
                        num = int(split[1])
                    except:
                        num = -1

                    if(num == -1):
                        self.debug("checkEconomyFile(): found invalid balance '{}', deleting row".format(split[1]))
                    else:
                        self.ecoBalances[split[0]] = num
        file.close()

    def ecoSaveToFile(self):
        file = self.getFile(self.ecoFileName, "w")

        for keyUID in self.ecoBalances:
            file.write("{}:{}\n".format(keyUID, self.ecoBalances[keyUID]))

        file.close()



#-----------------------------------------------------------
#--------------ROULETTE HELPER METHODS----------------------


    def roulGetNet(self, targetUID):
        statsFile = self.getFile(self.roulStatsFile, "r")

        found = False

        for line in statsFile:
            line = line.rstrip()

            split = line.rsplit(":")

            uid, val = split[0], split[1]

            if(uid == targetUID):
                found = True
                net = int(val)
                break

        if(found == False):
            net = 0

        return net


    def roulSetNet(self, targetUID, toSet):
        statsFile = self.getFile(self.roulStatsFile, "r")

        allLines = []

        for line in statsFile:
            line = line.rstrip()

            split = line.rsplit(":")

            uid, val = split[0], split[1]

            if(uid != targetUID):
                allLines.append(line)

        statsFile.close()

        statsFile = self.getFile(self.roulStatsFile, "w")

        for line in allLines:
            statsFile.write(line+"\n")

        statsFile.write("{}:{}\n".format(targetUID, toSet))

        statsFile.close()


    def roulChangeNet(self, targetUID, step):
        currentNet = self.roulGetNet(targetUID)
        self.roulSetNet(targetUID, currentNet + step)


#-----------------------------------------------------------
#-----------------HIGHLIGHT KEYWORDS------------------------

    def filterHighlightWords(self, schid, targetMode, targetID, fromName, message):
        if(self.isMe(schid, self.getClientIDByName(schid, fromName))):
            return
        foundKeywords = self.containsHighlightKeyword(message)
        if(foundKeywords != -1):
            strTimestamp = datetime.now().strftime("<%d.%m.%y %H:%M:%S>")

            file = self.getFile(self.highlightSaveFileName, "a")
            try:
                file.write("{} {} wrote {}: {}\n".format(strTimestamp, fromName, ("in private" if targetMode == 1 else "in channel '{}'".format(ts3lib.getChannelVariableAsString(schid, targetID, ts3defines.ChannelProperties.CHANNEL_NAME)[1])), message))
            except:
                self.debug("Failed to write highlighted message to file.")
            file.close()


            for keyword in foundKeywords:
                count = 0
                while self.findnth(message, keyword, count) != -1:
                    index = self.findnth(message, keyword, count)
                    pattern = message[index:index+len(keyword)]
                    message = message.replace(pattern, "[color=#cc0000][B]{}[/B][/color]".format(pattern))
                    count += 1



            total = "[color=#222299]" + strTimestamp + "[/color] [color=#2222cc][B]" + fromName + "[/B] wrote " + ("in private" if targetMode == 1 else "in channel '{}'".format(ts3lib.getChannelVariableAsString(schid, targetID, ts3defines.ChannelProperties.CHANNEL_NAME)[1])) + "[/color]: "+message

            if(len(total) >= 1024):
                total = total[0:1023]

            ts3lib.requestSendPrivateTextMsg(schid, total, self.getMyID(schid))

    def addHighlightKeyword(self, keyword):
        file = self.getFile(self.highlightFileName, "r")

        for line in file:
            line = line.rstrip()
            if(line == keyword):
                return -1

        file.close()
        file = self.getFile(self.highlightFileName, "a")
        file.write("{}\n".format(keyword))
        file.close()

    def removeHighlightKeyword(self, keyword):
        allKeywords = []

        file = self.getFile(self.highlightFileName, "r")

        found = -1

        for line in file:
            line = line.rstrip()
            if(line != keyword):
                found = True
                allKeywords.append(line)

        file.close()
        file = self.getFile(self.highlightFileName, "w")

        for keyword in allKeywords:
            file.write("{}\n".format(keyword))

        file.close()

        return found

    def containsHighlightKeyword(self, text):
        file = self.getFile(self.highlightFileName, "r")

        foundKeywords = []

        text = text.lower()

        for line in file:
            line = line.rstrip()
            if(line in text):
                foundKeywords.append(line)

        if(len(foundKeywords) == 0):
            return -1
        else:
            return foundKeywords

#-----------------------------------------------------------
#-------------------YOUTUBE LOADER--------------------------
    def ytNetworkReply(self, reply):
        pageContent = reply.readAll().data().decode("utf-8")

        info = self.ytGetImportantInfo(pageContent)

        #self.debug("got to reply")
        #self.debug("info: '{}'".format(info))

        schid = self.ytResponseTarget[0]
        targetMode = self.ytResponseTarget[1]
        targetID = self.ytResponseTarget[2]
        senderName = self.ytResponseTarget[3]
        url = self.ytResponseTarget[4]
        short = self.ytResponseTarget[5]

        startingTimeString = ""

        #if("&feature=" in url):
        #    self.sendTextMessage(schid, targetMode, "Please avoid using youtube link containing \"feature=youtu.be\"", targetID)
        #    self.ytResponseTarget = []
        #    return

        if(len(self.ytResponseTarget) == 7):
            time = self.ytResponseTarget[6]
            startingTimeString = " starting at [B]{}[/B]".format(time)
            url = "{}?t={}".format(url, time)

        elif("&t=" in url):
            time = url[url.find("&t=")+3:]
            startingTimeString = " starting at [B]{}[/B]".format(time)

        self.ytResponseTarget = []

        #self.debug("url: '{}', info '{}'".format(url, info))

        if(info == -1):
            self.sendTextMessage(schid, targetMode, "The given link is not a youtube video: [url]{}[/url]".format(url), targetID)
        else:
            timeSeconds = info[self.YT_TIME_IN_SECONDS]
            timeFormatted = self.formatSecondsToTimeString(timeSeconds)

            if(timeFormatted == "00:00"):
                timeFormatted = "Livestream"


            likes = int(info[self.YT_LIKES].replace(".", ""))
            dislikes = int(info[self.YT_DISLIKES].replace(".", ""))

            if(dislikes == 0 and likes == 0):
                ratio = "N/A"
                color = "ff0000"
            else:
                ratio = int((likes / (likes + dislikes))*100)
                if(ratio >= 95):
                    color = "00aa00"
                elif(ratio >= 85):
                    color = "bbbb00"
                else:
                    color = "ff0000"

            if(ratio == 100):
                colorString = "[color=#22ee22]↑({}%)↑[/color]".format(ratio)
            else:
                colorString = "[color=#{}]({}%)[/color]".format(color, ratio)


            self.statsIncrement("All", "YouTube links fetched", 1, 3)

            if(short == True):
                self.sendTextMessage(schid, targetMode,
                                     "[color=#aa0000]YouTube[/color] ⇒ [url={}]{}[/url] [{}{}] from [B]{}[/B] | [color=#00aa00]↑{}[/color] | [color=#ff0000]↓{}[/color] | {}".format(url,
                                                                                                                                                   info[self.YT_TITLE],
                                                                                                                                                   timeFormatted,
                                                                                                                                                   startingTimeString,
                                                                                                                                                   info[self.YT_CHANNEL],
                                                                                                                                                   info[self.YT_LIKES],
                                                                                                                                                   info[self.YT_DISLIKES],
                                                                                                                                                   colorString,
                                                                                                                                                   ratio), targetID)
            else:
                self.sendTextMessage(schid, targetMode, "[color=#aa0000]YouTube[/color] ⇒ [url={}]{}[/url] [{}{}] from [B]{}[/B]".format(url, info[self.YT_TITLE], timeFormatted, startingTimeString, info[self.YT_CHANNEL]), targetID)
                self.sendTextMessage(schid, targetMode, "[color=#aa0000]YouTube[/color] ⇒ {}".format(info[self.YT_DATE]), targetID)
                self.sendTextMessage(schid, targetMode,
                                     "[color=#aa0000]YouTube[/color] ⇒ [B]{}[/B] | [color=#00aa00]↑{}[/color] | [color=#ff0000]↓{}[/color] | {}".format(info[self.YT_VIEWS],
                                                                                                                         info[self.YT_LIKES],
                                                                                                                         info[self.YT_DISLIKES],
                                                                                                                         colorString,
                                                                                                                         ratio), targetID)

    def ytGetImportantInfo(self, pageContent):
        info = []

        content = str(pageContent)

        #self.debug("----\n----\n----\n----{}----\n----\n----\n----\n----".format(content))

        if(content == "" or content.find("Dieses Video existiert nicht.") != -1):
            return -1

        title = content[content.find("name=\"title\"")+22:self.findafter(content, "\">", content.find("name=\"title\"")+22)]
        title = self.addChromeSpecialChars(title)
        channel = content[content.find("\"name\": \"")+9:self.findafter(content, "\"", content.find("\"name\": \"")+9)]
        channel = self.addChromeSpecialChars(channel)
        date = content[content.find("<strong class=\"watch-time-text\">")+32:self.findafter(content, "</strong>", content.find("<strong class=\"watch-time-text\">")+32)]
        timeSeconds = content[content.find("\"length_seconds\":\"")+18:self.findafter(content, "\"", content.find("\"length_seconds\":\"")+18)]
        views = content[content.find("<div class=\"watch-view-count\">")+30:self.findafter(content, "</div>", content.find("<div class=\"watch-view-count\">")+30)]
        likes = content[content[content.find("like-button-renderer-like-button like-button-renderer-like-button-unclicked"):].find("button-content\">")+content.find("like-button-renderer-like-button like-button-renderer-like-button-unclicked")+16:self.findafter(content, "</span>", content[content.find("like-button-renderer-like-button like-button-renderer-like-button-unclicked"):].find("button-content\">")+content.find("like-button-renderer-like-button like-button-renderer-like-button-unclicked")+16)]
        dislikes = content[content[content.find("like-button-renderer-dislike-button like-button-renderer-dislike-button-unclicked"):].find("button-content\">")+content.find("like-button-renderer-dislike-button like-button-renderer-dislike-button-unclicked")+16:self.findafter(content, "</span>", content[content.find("like-button-renderer-dislike-button like-button-renderer-dislike-button-unclicked"):].find("button-content\">")+content.find("like-button-renderer-dislike-button like-button-renderer-dislike-button-unclicked")+16)]

        #self.debug("title '{}' channel '{}' date '{}' timeSeconds '{}' views '{}' likes '{}' dislikes '{}'".format(title, channel, date, timeSeconds, views, likes, dislikes))

        try:
            timeSecondsNum = int(timeSeconds)
        except:
            timeSecondsNum = "N/A"

        return [title, channel, views, "Not yet implemented", timeSecondsNum, date, likes, dislikes]

#-----------------------------------------------------------
#-----------------TWITCH CLIP LOADER------------------------
    def twitchNetworkReply(self, reply):
        pageContent = reply.readAll().data().decode("utf-8")

        info = self.twitchGetImportantInfo(pageContent)

        schid = self.twitchResponseTarget[0]
        targetMode = self.twitchResponseTarget[1]
        targetID = self.twitchResponseTarget[2]
        senderName = self.twitchResponseTarget[3]
        url = self.twitchResponseTarget[4]


        #self.debug("schid '{}'".format(schid))
        #self.debug("targetMode '{}'".format(targetMode))
        #self.debug("targetID '{}'".format(targetID))
        #self.debug("senderName '{}'".format(senderName))
        #self.debug("url '{}'".format(url))


        #self.debug("info[0] '{}'".format(info[0]))
        #self.debug("info[1] '{}'".format(info[1]))

        self.twitchResponseTarget = []

        if(info == -1):
            self.sendTextMessage(schid, targetMode, "The given link is not a twitch clip: [url]{}[/url]".format(url), targetID)
        else:
            nameAndGame = info[self.TWITCH_NAME_AND_GAME][0:-15]
            name = nameAndGame.split(" Playing ")[0]
            game = nameAndGame.split(" Playing ")[1]
            self.sendTextMessage(schid, targetMode, "[color=#0000bb]Twitch[/color] ⇒ [url={}]{}[/url] | [B]{}[/B] playing [B]{}[/B]".format(url, info[self.TWITCH_TITLE], name, game), targetID)

    def twitchGetImportantInfo(self, pageContent):
        info = []

        content = str(pageContent)

        #self.debug("content tst: \n\n\n\n{}".format(content))

        if(content == "" or len(content) < 4000):
            return -1

        title = content[content.find("property='og:description' content='")+35:self.findafter(content, "'>", content.find("property='og:description' content='")+35)]
        title = title[0:title.rfind("-")-1]
        title = self.addChromeSpecialChars(title)
        nameAndGame = content[content.find("og:title' content='")+19:self.findafter(content, "'>", content.find("og:title\' content='")+19)]
        nameAndGame = self.addChromeSpecialChars(nameAndGame)

        return [title, nameAndGame]

#-----------------------------------------------------------
#--------------------CONFIG FILE----------------------------

    def prepareConfigVar(self):
        self.configLoadVars["Chat_Command_Prefix"] = ["chatCommandPrefix", "!"]
        self.configLoadVars["Chat_Commands_Enabled"] = ["chatCommandsEnabled", True]

        self.configLoadVars["Revenge_Poke_Enabled"] = ["revengePokeEnabled", False]
        self.configLoadVars["Revenge_Poke_Message"] = ["revengePokeMessage", "FUCK YOU"]

        self.configLoadVars["debug"] = ["debugPlugin", False]

        self.configLoadVars["No_Move_File_Name"] = ["noMoveFileName", "nomove.txt"]

        self.configLoadVars["Door_Enabled"] = ["doorEnabled", True]
        self.configLoadVars["Door_Kick_Message"] = ["doorKickMessage", "Left the house."]
        self.configLoadVars["Door_Channel_Name"] = ["doorChannelName", "Door"]

        self.configLoadVars["Highlight_Enabled"] = ["highlightEnabled", False]
        self.configLoadVars["Highlight_File_Name"] = ["highlightFileName", "highlightKeywords.txt"]
        self.configLoadVars["Highlight_Save_File_Name"] = ["highlightSaveFileName", "highlightSave.txt"]

        self.configLoadVars["AFK_Enabled"] = ["afkEnabled", True]
        self.configLoadVars["AFK_Prefix"] = ["afkPrefix", "[afk] "]
        self.configLoadVars["AFK_Message"] = ["afkMessage", "I am currently afk, please try again later!"]
        self.configLoadVars["AFK_Bot_Tag"] = ["afkBotTag", "[Bot Response]: "]

        self.configLoadVars["Eco_Enabled"] = ["ecoEnabled", True]
        self.configLoadVars["Eco_Enabled_For_Server_UID"] = ["ecoEnabledForServerUID", "-None-"]
        self.configLoadVars["Eco_Tick_Time_Seconds"] = ["ecoTickTimeInSeconds", 3600]
        self.configLoadVars["Eco_File_Name"] = ["ecoFileName", "ecoBalances.txt"]
        self.configLoadVars["Eco_Point_Limit"] = ["ecoPointLimit", 100]
        self.configLoadVars["Eco_Excluded_Max_Send_Amount"] = ["ecoExcludedMaxSendAmount", 20]

        self.configLoadVars["Bet_Enabled"] = ["betEnabled", True]
        self.configLoadVars["Bet_Min_Amount"] = ["betMinAmount", 1]
        self.configLoadVars["Bet_Max_Amount"] = ["betMaxAmount", 4]

        self.configLoadVars["Roulette_Enabled"] = ["roulEnabled", True]
        self.configLoadVars["Roulette_Win_Chance_Percent"] = ["roulWinChancePercent", 50]
        self.configLoadVars["Roulette_Win_Multiplier"] = ["roulWinMultiplier", 2]
        self.configLoadVars["Roulette_Cooldown"] = ["roulCooldown", "5m"]
        self.configLoadVars["Roulette_Special_Win_Sound"] = ["roulSpecialWinSound", ""]

        self.configLoadVars["Anti_Server_Kick_Enabled"] = ["rcEnabled", False]
        self.configLoadVars["Anti_Server_Kick_Revenge_Kick_Enabled"] = ["rcRevengeKickEnabled", False]
        self.configLoadVars["Anti_Server_Kick_Revenge_Kick_Message"] = ["rcRevengeKickMessage", "'!kickmsg!' my ass"]
        self.configLoadVars["Anti_Server_Kick_Delay"] = ["rcDelay", 0]

        self.configLoadVars["Anti_Corruption_Enabled"] = ["acEnabled", False]
        self.configLoadVars["Anti_Corruption_Server_Group_ID"] = ["acAdminGroupID", 0]
        self.configLoadVars["Anti_Corruption_Priv_Key"] = ["acPrivKey", "-privkey-"]


        self.configLoadVars["Token_Enabled"] = ["tokenEnabled", True]
        self.configLoadVars["Token_Gag_Group_ID"] = ["tokenGagGroupID", 0]
        self.configLoadVars["Token_Sticky_Group_ID"] = ["tokenStickyGroupID", 0]
        self.configLoadVars["Token_Mute_Group_ID"] = ["tokenMuteGroupID", 0]

        self.configLoadVars["Now_Playing_Enabled"] = ["npEnabled", False]
        self.configLoadVars["Now_Playing_Stream_Companion_File_Path"] = ["npSCFilePath", "-stream companion file path-"]

        self.configLoadVars["TTS_Enabled"] = ["ttsEnabled", False]
        self.configLoadVars["TTS_Volume"] = ["ttsVolume", 1.0]
        self.configLoadVars["TTS_Characters_Per_Point"] = ["ttsCharsPerPoint", 10.0]
        self.configLoadVars["TTS_Cooldown_In_Seconds_Per_Character"] = ["ttsCDPerChar", 0.8]
        self.configLoadVars["TTS_Min_Cooldown"] = ["ttsMinCD", 10]
        self.configLoadVars["TTS_Max_Cooldown"] = ["ttsMaxCD", 100]
        self.configLoadVars["TTS_Max_Characters"] = ["ttsMaxChars", 256]
        self.configLoadVars["TTS_Unicode_Increase"] = ["ttsUnicodeIncrease", 0.4]
        self.configLoadVars["TTS_Global_Output"] = ["ttsGlobalOutput", "(i)7"]
        self.configLoadVars["TTS_Local_Output"] = ["ttsLocalOutput", "(i)5"]

        self.configLoadVars["Quick_Chat_Enabled"] = ["quickChatEnabled", True]
        self.configLoadVars["Quick_Chat_1"] = ["quickChat1", "Yes!"]
        self.configLoadVars["Quick_Chat_2"] = ["quickChat2", "No!"]
        self.configLoadVars["Quick_Chat_3"] = ["quickChat3", "Fuck you!"]

        self.configLoadVars["Playsound_Enabled"] = ["psEnabled", True]
        self.configLoadVars["Playsound_Default_Volume"] = ["psDefaultVolume", 1.0]
        self.configLoadVars["Playsound_Default_Speed"] = ["psDefaultSpeed", 1.0]

        self.configLoadVars["Daily_Enabled"] = ["dailyEnabled", True]
        self.configLoadVars["Daily_Reward"] = ["dailyReward", 9]
        self.configLoadVars["Daily_Cooldown"] = ["dailyCooldown", "7:00"]
        self.configLoadVars["Daily_Use_Cooldown_As_Reset_Time"] = ["dailyUseCooldownAsResetTime", True]

        self.configLoadVars["FloodPrevention_Enabled"] = ["fpEnabled", False]
        self.configLoadVars["FloodPrevention_Max_Pokes_Per_Minute"] = ["fpMaxPokesPerMinute", 10]
        self.configLoadVars["FloodPrevention_Max_Privates_Per_Minute"] = ["fpMaxPrivatePerMinute", -1]
        self.configLoadVars["FloodPrevention_Max_Commands_Per_Minute"] = ["fpMaxCommandsPerMinute", -1]

        self.configLoadVars["Immediate_Link_Reaction"] = ["immediateLinkReaction", True]
        self.configLoadVars["Reset_Config"] = ["initConfigVar", "No"]


        resetConfig = self.getStringFromConfig("Reset_Config")
        #self.debug("\nresetConfig '{}'\n".format(resetConfig))
        if(resetConfig != "No"):
            self.initConfigVar = "Yes"


    def initConfig(self):
        if(self.initConfigVar == "No"):
            return
        self.debug("Resetting config... '{}'".format(self.initConfigVar))

        for keyVarName in self.configLoadVars:
            self.writeToConfig(keyVarName, self.configLoadVars[keyVarName][1])

    def loadConfig(self):
        for keyVarName in self.configLoadVars:
            varName = self.configLoadVars[keyVarName][0]
            varType = self.configLoadVars[keyVarName][1]

            if(type(varType) == type(True)):
                setattr(self, varName, self.getBooleanFromConfig(keyVarName))
                #self.debug("(Boolean) Set variable for '{}' to '{}'".format(keyVarName, self.getBooleanFromConfig(keyVarName)))
            elif(type(varType) == type("string")):
                setattr(self, varName, self.getStringFromConfig(keyVarName))
            elif(type(varType) == type(123)):
                setattr(self, varName, self.getIntFromConfig(keyVarName))
            elif(type(varType) == type(1.5)):
                setattr(self, varName, self.getFloatFromConfig(keyVarName))
            else:
                self.debug("Failed to load config variable '{}' as it's type is not one of (Boolean, String, Integer, Float).")


            if(getattr(self, varName) == "{no_key_found}"):
                setattr(self, varName, self.configLoadVars[keyVarName][1])
                self.debug("[color=#ff0000]'{}' was not found in the config, setting it to default value of '{}'[/color]".format(keyVarName, self.configLoadVars[keyVarName][1]))

            #self.debug("keyVarName '{}' [0] '{}' [1] '{}'".format(keyVarName, self.configLoadVars[keyVarName][0], self.configLoadVars[keyVarName][1]))

    def saveConfig(self):
        for keyVarName in self.configLoadVars:
            self.writeToConfig(keyVarName, getattr(self, self.configLoadVars[keyVarName][0]))
            #self.debug("Wrote '{}' to config with value '{}'".format(keyVarName, getattr(self, self.configLoadVars[keyVarName][0])))


    def getStringFromConfig(self, key):
        configFile = self.getFile(self.configFileName, "r")

        for zeile in configFile:
            split = zeile.rstrip().split("=", 1)
            if(split[0] == key):
                configFile.close()
                return split[1]

        configFile.close()
        return "{no_key_found}"

    def getIntFromConfig(self, key):
        configFile = self.getFile(self.configFileName, "r")

        for zeile in configFile:
            split = zeile.rstrip().split("=", 1)
            if(split[0] == key):
                configFile.close()
                try:
                    val = int(split[1])
                    return val
                except:
                    return "{no_key_found}"


        configFile.close()
        return "{no_key_found}"

    def getFloatFromConfig(self, key):
        configFile = self.getFile(self.configFileName, "r")

        for zeile in configFile:
            split = zeile.rstrip().split("=", 1)
            if(split[0] == key):
                configFile.close()
                try:
                    val = float(split[1])
                    return val
                except:
                    return "{no_key_found}"


        configFile.close()
        return "{no_key_found}"

    def getBooleanFromConfig(self, key):
        configFile = self.getFile(self.configFileName, "r")

        for zeile in configFile:
            split = zeile.rstrip().split("=", 1)
            if(split[0] == key):
                configFile.close()
                if(split[1] == "False"):
                    return False
                elif(split[1] == "True"):
                    return True
                else:
                    break


        configFile.close()
        return "{no_key_found}"

    def writeToConfig(self, key, value, replace=True):
        configFile = self.getFile(self.configFileName, "r")

        prevList = []

        for zeile in configFile:
            prevList.append(zeile.rstrip())

        configFile.close()
        configFile = self.getFile(self.configFileName, "w")

        insertedKey = False

        for string in prevList:
            split = string.split("=")
            if(split[0] == key):
                insertedKey = True
                if(replace == True):
                    configFile.write("{}={}\n".format(key, value))
                else:
                    configFile.write("{}\n".format(string))
            else:
                configFile.write("{}\n".format(string))

        if(insertedKey == False):
            configFile.write("{}={}\n".format(key, value))

        configFile.close()

#-----------------------------------------------------------
#-------------------Helper Methods--------------------------

    def timedCall(self, method, cmdParameters, parameters, times, timerStorage=None, ignoreOfflineCheck=False):# cmdParameters = [schid, targetMode, targetID, senderName]
        miliseconds, seconds, minutes, hours, days, weeks = times[0], times[1], times[2], times[3], times[4], times[5]

        #self.debug("\nreached timedCall")
        if(ignoreOfflineCheck == False):
            #self.debug("offline checking...")
            uid = self.getClientUIDByName(cmdParameters[0], cmdParameters[3])
            if(uid == -1):
                self.debug("Client is no longer on the server...")
                return
            cmdParameters[3] = uid


        totalInSeconds = self.convertTimeValuesToSeconds(miliseconds=miliseconds, seconds=seconds, minutes=minutes, hours=hours, weeks=weeks)

        timer = QTimer()
        timer.setSingleShot(True)
        timer.timeout.connect(self.shootTimer)

        if(timerStorage == None):
            self.singleShotParams[timer] = [method, cmdParameters, parameters, ignoreOfflineCheck]
        else:
            timerStorage[timer] = [method, cmdParameters, parameters, ignoreOfflineCheck]
        timer.start(round(totalInSeconds * 1000))

    def shootTimer(self):#listParams -> [removeTimedToken, [schid, targetMode.CLIENT, targetID, targetName(UID hier)], [params]]
        #self.debug("got here")
        for timerStorage in self.timerStorages:
            for timer in timerStorage:
                #self.debug("got here aswell '{}'".format(timer.isActive()))
                if(timer.isActive() == False):
                    #self.debug("found timer")
                    listParams = timerStorage[timer]
                    del(timerStorage[timer])
                    method = listParams[0]
                    cmdParameters = listParams[1]
                    parameters = listParams[2]
                    ignoreOfflineCheck = listParams[3]

                    if(ignoreOfflineCheck == False):
                        clientName = self.getClientNameByUID(cmdParameters[0], cmdParameters[3])

                        if(clientName == -1):
                            self.debug("[color=#ff0000]A client who issued the command [B]'{}'[/B] is no longer online, timer event canceled.[/color]".format(method.__name__))
                            return

                        cmdParameters[3] = clientName

                    method(*cmdParameters, parameters)
                    return
        self.debug("no timer is dead????")


    def getTimesFromTimeString(self, timeString):#5h60s2m3500f
        fromIndex = 0
        hasTimeUnit = False

        dictTimes = {}

        for toIndex in range(1, len(timeString)+1):
            try:
                num = int(timeString[fromIndex:toIndex])
                hasTimeUnit = False
            except:
                if(fromIndex == toIndex):
                    return -69  #what in the world happened here
                else:
                    try:
                        num = int(timeString[fromIndex:toIndex-1])
                        unit = timeString[toIndex-1:toIndex]
                        fromIndex = toIndex
                        if(unit in dictTimes):
                            dictTimes[unit] += num
                        else:
                            dictTimes[unit] = num
                        hasTimeUnit = True
                    except:
                        return -1

        if(hasTimeUnit == False):
            return -2

        weeks = 0
        days = 0
        hours = 0
        minutes = 0
        seconds = 0
        miliseconds = 0

        for key in dictTimes:
            if(key == "f"):
                miliseconds = dictTimes[key]
            elif(key == "s"):
                seconds = dictTimes[key]
            elif(key == "m"):
                minutes = dictTimes[key]
            elif(key == "h"):
                hours = dictTimes[key]
            elif(key == "d"):
                days = dictTimes[key]
            elif(key == "w"):
                weeks = dictTimes[key]
            else:
                return key

        return [miliseconds, seconds, minutes, hours, days, weeks]

    def convertTimeValuesToSeconds(self, miliseconds=0, seconds=0, minutes=0, hours=0, days=0, weeks=0):
        totalInSeconds = 0
        if(weeks != 0):
            totalInSeconds += weeks * 7 * 24 * 3600
        if(days != 0):
            totalInSeconds += days * 24 * 3600
        if(hours != 0):
            totalInSeconds += hours * 3600
        if(minutes != 0):
            totalInSeconds += minutes * 60
        if(seconds != 0):
            totalInSeconds += seconds
        if(miliseconds != 0):
            totalInSeconds += miliseconds / 1000
        return totalInSeconds

    def formatSecondsToTimeString(self, timeSeconds, mode=0):
        if(mode == 0):
            if(timeSeconds != "N/A"):
                hours = -1
                minutes = -1
                seconds = -1

                if(timeSeconds >= 3600):
                    hours = int(timeSeconds / 3600)
                    timeSeconds = timeSeconds % 3600
                if(timeSeconds >= 60):
                    minutes = int(timeSeconds / 60)
                    timeSeconds = timeSeconds % 60
                if(timeSeconds >= 0):
                    seconds = timeSeconds

                timeFormatted = ""

                if(hours != -1):
                    timeFormatted = "{}:".format(hours)

                if(minutes != -1):
                    if(minutes < 10):
                        timeFormatted = "{}0{}:".format(timeFormatted, minutes)
                    else:
                        timeFormatted = "{}{}:".format(timeFormatted, minutes)
                else:
                    timeFormatted = "{}00:".format(timeFormatted)

                if(seconds != -1):
                    if(seconds < 10):
                        timeFormatted = "{}0{}".format(timeFormatted, seconds)
                    else:
                        timeFormatted = "{}{}".format(timeFormatted, seconds)
            else:
                timeFormatted = "N/A"

            return timeFormatted

        elif(mode == 1):
            seconds = timeSeconds % 60
            timeSeconds = int(timeSeconds / 60)

            minutes = timeSeconds % 60
            timeSeconds = int(timeSeconds / 60)

            hours = timeSeconds

            timeFormatted = ""

            if(hours != 0):
                timeFormatted = "{}h".format(hours)

            if(timeFormatted != ""):
                timeFormatted = "{} {}m".format(timeFormatted, minutes)
            else:
                if(minutes != 0):
                    timeFormatted = "{}m".format(minutes)

            if(timeFormatted != ""):
                timeFormatted = "{} {}s".format(timeFormatted, seconds)
            else:
                timeFormatted = "{}s".format(seconds)

            return timeFormatted

        return ""


    def parseVariableFromString(self, string):
        string = string.strip()

        if(string.startswith("\"")):#Case String
            if(not string.endswith("\"")):
                self.debug("Parse error: String end not found in: '{}'".format(string))
                return None
            return string[1:-1].replace("\\\"", "\"")

        elif(string == "True" or string == "False"):#Case Boolean
            if(string == "True"):
                return True
            else:
                return False

        elif("." in string):#Case Float
            try:
                val = float(string)
                return val
            except:
                self.debug("Parse error: Incorrect float format: '{}'".format(string))
                return None

        elif(string.startswith("[")):#Case List
            if(not string.endswith("]")):
                self.debug("Parse error: List end not found in: '{}'".format(string))
                return None
            string = string[1:-1]

            split = self.revertSplitForLists(string.split(","))
            #self.debug("split '{}'".format(split))
            if(split == None):
                self.debug("Parse error: List exception: unusual brackets or string escape characters found in: '{}'".format(string))
                return None

            newList = []
            for var in split:
                newList.append(self.parseVariableFromString(var))
            return newList

        elif(string.startswith("{")):#Case Dictionary
            if(not string.endswith("}")):
                self.debug("Parse error: Dictionary end not found in: '{}'".format(string))
                return None
            string = string[1:-1]

            splitComma = self.revertSplitForLists(string.split(","))
            if(splitComma == None):
                self.debug("Parse error: List exception: unusual brackets or string escape characters found in: '{}'".format(string))
                return None

            diction = {}
            for dictEntry in splitComma:
                splitColon = dictEntry.split(":")
                if(len(splitColon) != 2):
                    self.debug("Parse error: Dictionary entry format not correct: multiple colons (':') found in: '{}'".format(string))
                    return None
                key = self.parseVariableFromString(splitColon[0].strip())
                value = self.parseVariableFromString(splitColon[1].strip())
                if(key in diction):
                    self.debug("Parse error: Dictionary key not unique found in: '{}'".format(string))
                    return None
                diction[key] = value
            return diction

        elif(string.startswith("(")):#Case Tuple
            if(not string.endswith(")")):
                self.debug("Parse error: Tuple end not found in: '{}'".format(string))
                return None
            string = string[1:-1]

            split = self.revertSplitForLists(string.split(","))
            if(split == None):
                self.debug("Parse error: List exception: unusual brackets or string escape characters found in: '{}'".format(string))
                return None

            newList = []
            for var in split:
                newList.append(self.parseVariableFromString(var))
            return tuple(newList)

        else:#Case Integer
            try:
                integer = int(string)
                return integer
            except:
                self.debug("Parse error: Integer not parsable in: '{}'".format(string))
                return None


    def revertSplitForLists(self, pList):# ["'key' : [1"," 2"," 3"," 'Another string']"," 'key2' : True"] ->
        countAngular = 0
        countRound = 0
        countCurly = 0
        inString = False

        completeList = []
        completePart = ""
        step = 0
        for stringPart in pList:
            #self.debug("{}: stringPart: {}".format(step, stringPart))
            if(countAngular != 0 or countRound != 0 or countCurly != 0 or inString == True):
                if(completePart == ""):
                    completePart = stringPart
                else:
                    completePart += ","+stringPart
            else:
                completePart += stringPart
            #self.debug("{}: completePart: {}".format(step, completePart))

            countAngular += stringPart.count("[")
            countAngular -= stringPart.count("]")

            countRound += stringPart.count("(")
            countRound -= stringPart.count(")")

            countCurly += stringPart.count("{")
            countCurly -= stringPart.count("}")

            if((completePart.count("\"")%2 - completePart.count("\\\"")) == 0):
                inString = False
            if((completePart.count("\"")%2 - completePart.count("\\\"")) == 1):
                inString = True

            if(countAngular == 0 and countRound == 0 and countCurly == 0 and inString == False):
                completeList.append(completePart)
                completePart = ""
            #self.debug("{}: completeList: {}".format(step, completeList))
            step += 1


        return completeList

    def pafyLookup(self, url, action="important"):
        try:
            #self.debug("given url: {}, action = {}".format(url, action))

            video = pafy.new(url, gdata=True)

            #self.debug("3")

            ret = {}

            if(action == "title" or action == "important" or action == "all"):
                ret["Title"] = video.title

            if(action == "author" or action == "important" or action == "all"):
                ret["Author"] = video.author

            if(action == "duration" or action == "important" or action == "all"):
                ret["Duration"] = video.duration

            if(action == "length"):
                ret["Length"] = video.length

            if(action == "likes" or action == "important" or action == "all"):
                ret["Likes"] = video.likes

            if(action == "dislikes" or action == "important" or action == "all"):
                ret["Dislikes"] = video.dislikes

            if(action == "viewcount" or action == "all"):
                ret["Viewcount"] = video.viewcount

            if(action == "published" or action == "all"):
                ret["Published"] = video.published

            if(action == "description" or action == "all"):
                ret["Description"] = video.description

            #self.debug("4: {}".format(ret))

            return ret

        except Exception as err:
            self.debug("An exception of type {0} occurred. Arguments:\n{1!r}".format(type(err).__name__, err.args))
            #self.debug("Traceback: {}".format(traceback.format_exc()))
            return "ERROR: Could not parse url."



    def textToSpeech(self, text, language="detect", translateTo="none", local=False, setup=False):
        if(setup == True):
            self.ttsProcessStorage = subprocess.Popen(["python", self.getPathToFile("tts.pyw"), "setup"])
            return

        if(local == True):
            device = self.ttsLocalOutput
        else:
            device = self.ttsGlobalOutput

        self.ttsProcessStorage = subprocess.Popen(["pythonw", self.getPathToFile("tts.pyw"), "lang="+language, "trans="+translateTo, "device={}".format(device), "vol={}".format(self.ttsVolume), text])



    def getUnknownParamsMessage(self, cmd):
        return "[color=#ff0000]Unknown parameters.[/color]\n -> Usage of {}: [B]\"{}\"[/B]".format(cmd, self.getCommandUsage(cmd))


    def addChromeSpecialChars(self, string):
        for special in self.chromeSpecialChars:
            string = string.replace(special, self.chromeSpecialChars[special])

        return string

    def amAfk(self, schid):
        if(self.afkEnabled == False):
            return False

        (err, myName) = ts3lib.getClientVariableAsString(schid, self.getMyID(schid), ts3defines.ClientProperties.CLIENT_NICKNAME)
        #self.debug("myName: '{}', afkPrefix: '{}'".format(myName, self.afkPrefix))
        if(myName.startswith(self.afkPrefix) or myName.endswith(self.afkPrefix)):
            return True
        else:
            return False

    def getMyName(self, schid):
        (err, myName) = ts3lib.getClientVariableAsString(schid, self.getMyID(schid), ts3defines.ClientProperties.CLIENT_NICKNAME)

        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return myName

    def getPathToFile(self, fileName):
        directory = os.path.dirname(__file__)
        joined = os.path.join(directory, fileName)
        return joined

    def getFile(self, fileName, mode, encoding="cp1252"):
        try:
            file = open(self.getPathToFile(fileName), "r", encoding=encoding)
        except IOError:
            file = open(self.getPathToFile(fileName), "w", encoding=encoding)
        finally:
            file.close()

        try:
            file = open(self.getPathToFile(fileName), mode, encoding=encoding)
        except IOError:
            self.debug("something really weird happened here...")
            return -1
        #self.debug("File encoding: '{}'".format(file.encoding))
        return file

    def createNewFile(self, fileName):
        try:
            file = open(self.getPathToFile(fileName), "w")
        except IOError:
            return false
        else:
            file.close()
            return true

    def getMyID(self, schid):
        (err, myID) = ts3lib.getClientID(schid)
        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return myID

    def getMyChannelID(self, schid):
        (err, myChannelID) = ts3lib.getChannelOfClient(schid, self.getMyID(schid))
        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return myChannelID

    def getClientNameFromSubName(self, schid, subname):
        (err, clientList) = ts3lib.getClientList(schid)

        foundNames = []

        for clientID in clientList:

            (err, clientName) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_NICKNAME)
            (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)

            if(subname == clientName):
                return subname
            elif(subname == clientUID):
                return clientName
            if(subname.lower() in clientName.lower()):
                foundNames.append(clientName)

        if(len(foundNames) == 0):
            return -1
        elif(len(foundNames) == 1):
            return foundNames[0]
        else:
            return foundNames

    def getClientIDByName(self, schid, name):
        (err, clientList) = ts3lib.getClientList(schid)

        for clientID in clientList:

            (err, clientName) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_NICKNAME)

            if(clientName == name):
                return clientID

        return -1

    def getClientUIDByName(self, schid, name):
        clientID = self.getClientIDByName(schid, name)

        if(clientID == -1):
            return -1

        (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)

        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return clientUID


    def getClientUIDByID(self, schid, clientID):
        (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)

        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return clientUID


    def getClientIDByUID(self, schid, uid):
        (err, clientList) = ts3lib.getClientList(schid)

        for clientID in clientList:
            (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
            if(uid == clientUID):
                return clientID
        return -1


    def getClientNameByUID(self, schid, uid):
        (err, clientList) = ts3lib.getClientList(schid)

        for clientID in clientList:
            (err, clientUID) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_UNIQUE_IDENTIFIER)
            if(uid == clientUID):
                (err, clientName) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_NICKNAME)
                return clientName
        return -1

    def getCurrentSCHID(self):
        return ts3lib.getCurrentServerConnectionHandlerID()


    def getClientNameByID(self, schid, clientID):
        (err, clientName) = ts3lib.getClientVariableAsString(schid, clientID, ts3defines.ClientProperties.CLIENT_NICKNAME)

        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return clientName

    def getMyNameByID(self, schid):
        myID = self.getMyID(schid)
        return self.getClientNameByID(schid, myID)

    def getChannelNameByID(self, schid, channelID):
        (err, channelName) = ts3lib.getChannelVariableAsString(schid, channelID, ts3defines.ChannelProperties.CHANNEL_NAME)
        if(err != ts3defines.ERROR_ok):
            return -1
        else:
            return channelName

    def getChannelIDByName(self, schid, searchName):
        (err, allChannels) = ts3lib.getChannelList(schid)
        allFoundIDs = []
        allFoundNames = []
        for id in allChannels:
            (err, channelName) = ts3lib.getChannelVariableAsString(schid, id, ts3defines.ChannelProperties.CHANNEL_NAME)
            if(searchName.lower() in channelName.lower()):
                if(searchName.lower() == channelName.lower()):
                    return id
                allFoundIDs.append(id)
                allFoundNames.append(channelName)

        if(len(allFoundIDs) == 0):
            return -1
        elif(len(allFoundIDs) == 1):
            return allFoundIDs[0]
        else:
            return allFoundNames


    def isMe(self, schid, clientID):
        if(self.getMyID(schid) == clientID):
            return True
        else:
            return False

    def isMeByUID(self, clientUID):
        for schid in ts3lib.getServerConnectionHandlerList()[1]:
            myID = self.getMyID(schid)
            myUID = self.getClientUIDByID(schid, myID)
            if(clientUID == myUID):
                return True

        return False

    def debug(self, message):
        if self.debugPlugin: ts3lib.printMessageToCurrentTab(message)

    def max(self, *params):
        if(len(params) == 0):
            return None
        if(len(params) == 1):
            return params[0]


        highestVar = params[0]
        for i in range(1, len(params)):
            if(params[i] > highestVar):
                highestVar = params[i]

        return highestVar

    def min(self, *params):
        if(len(params) == 0):
            return None
        if(len(params) == 1):
            return params[0]


        lowestVar = params[0]
        for i in range(1, len(params)):
            if(params[i] < lowestVar):
                lowestVar = params[i]

        return lowestVar

    def findafter(self, haystack, needle, pos):
        return haystack[pos:].find(needle)+pos

    #find nth occurence of pattern in string
    def findnth(self, haystack, needle, n):
        haystack = haystack.lower()
        parts = haystack.split(needle, n+1)
        if len(parts)<=n+1:
            return -1
        return len(haystack)-len(parts[-1])-len(needle)
