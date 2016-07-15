 # 关于 ABOUT 
 基于websocket的TShock远程管理插件， 可在浏览器中管理TShock服务器    
 Web broswer tool for tshock remote manage, based on websocket!




# 使用 Usage   
* 编译后复制 WSRC_Plug.dll到TShock插件目录，运行服务器
* 复制 `server.html` 到你的服务器或本地电脑，在浏览器中打开， 按插件中WSRC_config.json文件的配置登录管理界面
* 进入后可 输入 `/wsc` 获取本插件的命令， 建议及时修改登录用户名、密码及端口
* Compile the project and copy WSRC_Plugin.dll to TShock plugin`path, run the server
* copy `server.html` to your webserver or you pc, launch client from webbroswer and login to manager
* login and type `wsc` to get this plunin`command, remember to change your username、 password and port for security



# 问题 ISSUE
* 编译时将 `Fleck.dll` 作为内嵌资源编译， 但在 `Mono` 环境中却不能正确执行， 还需将 `Fleck.dll` 复制到插件目录
* 用于管理的 `HTML`  页面还需进一步优化
* Compile `Fleck.dll` as embed resource, but it does not work on `Mono`, so you need copy `Fleck.dll` to olugins path if you run server at `Mono` 
* The `HTML` page need optimize

