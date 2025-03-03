# cs2_WorkshopDefaultMap
A CSSharp plugin to set default map after server restart till valve fixes it.

Configuration:
1) Place the latest release in your plugins folder and (re)start the server. This is to generate config file in `addons/counterstrikesharp/configs/WorkshopDefaultMap/WorkshopDefaultMap.json`.
2) Open the above json file in your favourite text editor and add your workshop mapname or workshop mapid in `Map` or even some default valve map.
3) Restart the server.

> [!Caution]
> • If you added workshop mapname in the config file, make sure the map is in some workshop collection and you must have `host_workshop_collection` in your server startup line.

> [!Caution]
> • If you added workshop mapid in the config file, it isn't compulsory to add `host_workshop_collection` in your server startup line ~~but server might change the map to that specific map twice in this case (This will be the case only till CSSharp exposes `Server.WorkshopID` variable)~~
