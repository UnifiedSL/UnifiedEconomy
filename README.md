## UnifiedEconomy
Is a plugin, who wants to simplify the work of developers / owners by adding in a small framework everything you need.

This plugin helps out for the economy.

(NOTE: this is currently a framework in the near future, it will become a full plugin for server owner to use too)

## Setting up the plugin

Currently the plugin supports for databases
 - `Json` support local storage
 - `MongoDB` support local and atlas instances

(If you need any help, contact me on Exiled or in DM)
## For Developers

There are methods to help you out directly by Extensions with this you can call directly methods like
```c#
Player player = Player.Get(sender);

player.AddBalance(10f);
player.RemoveBalance(10f);
```

## For ScriptedEvents

Methods added for Scripted Events are
```yaml
UE_ADDMONEY <player> <money> (Adds balance to the player to remove it just add -)
UE_MONEY <player> (Gets the money)
```
