# nsupervisor

本项目衍生自 [WbtGurad](https://github.com/codeex/WbtGuard)，做了功能上的增强和改进，可用于方便的 windows/linux 下的服务进程的监控。

功能：
- 可以监视配置的进程，并跟踪其输出或错误到日志文件。
- 如果进程不存在，则启动指定的命令到子进程，可以携带参数、环境变量等。
- 每隔一定的事件就会扫描进程集，可由appsettings.json内的参数（CheckInterval）指定，默认为20s。
- 如果配置了路由进程的输出，则输出进程的控制台输出到指定文件。
- 服务本身的日志写在logs内
- 需要监视的进程配置在`services.ini`文件内，你可以修改并添加更多配置，配置节必须由`program:`开头，可以配置多个节，但是节名不能重复。


# 监视配置文件
参数仿照supervisor的配置文件。

```
[program:WebTest]
# 运行目录
directory=D:\WebTest\
# 监控的进程命令， 注意其进程名需与 上面配置的 /program: 后面相同，否则会重复启动多个程序。
command=D:\WebTest\WebTest.exe
stderr_logfile=D:\WebTest\stderr.log
stdout_logfile=d:\WebTest\stdout.log
arguments=
#多个环境变量用;分割。
env=

[program:WebTest2]
# 运行目录
directory=D:\WebTest2\
# 监控的进程命令， 注意其进程名需与 上面配置的 /program: 后面相同，否则会重复启动多个程序。
command=D:\WebTest2\WebTest.exe
stderr_logfile=D:\WebTest2\stderr.log
stdout_logfile=d:\WebTest2\stdout.log
arguments=
#多个环境变量用;分割。
env=
```

# 监视逻辑

鉴于原项目无法监视多个同名程序，这里修改了监视逻辑，新的监视逻辑如下：

- nsupervisor 启动时，对于每个监视配置，会检查进程是否存在，如果不存在，则启动一个新的进程。
- nsupervisor 启动新进程时，会记录该配置的名称、进程的 id 和启动时间，保存在数据库中。

由于数据库中保存了所有配置节最后启动的进程 id 和启动时间。因此，当重启时，nsupervisor 会检查数据库中的记录，根据进程 id 和启动时间，去匹配运行中的进程，如果未匹配到，则启动一个新的进程。

这样一来，只需要设置不同的配置节的名称，就可以监视同名进程了。且监视过程中，也不会干扰到用户打开新的同名程序。

# 管理页面
默认启动管理页面 http://localhost:8088 , 可以控制服务重启、停止、清理日志或查看日志（8K）.
![image](https://user-images.githubusercontent.com/3210368/211051096-37f96786-f3d0-4537-bce2-5d5eb881b123.png)

# 运行程序
```shell
nsupervisor.exe install --autostart 
nsupervisor.exe start
```

运行命令必须以管理员身份执行。
运行后会作为服务安装在系统内，可以查找 nsupervisor service.
如果需要卸载，可以使用`nsupervisor.exe uninstall`

# TODO

- [x] 修改监视逻辑，能够监视同名进程
- [ ] 替换成 blazor server 界面
- [ ] 适配 linux 环境