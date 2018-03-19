package main

import (
	// _ "captureorderfd/routers"
	_ "github.com/shanepeckham/captureorderack/routers"

	"github.com/astaxie/beego"
)

func main() {
	beego.BConfig.WebConfig.DirectoryIndex = true
	beego.BConfig.WebConfig.StaticDir["/swagger"] = "swagger"
	beego.Run()
}
