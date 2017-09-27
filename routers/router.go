// @APIVersion 1.0.0
// @Title Capture Order
// @Description Capture Order
// @Contact astaxie@gmail.com
// @TermsOfServiceUrl http://beego.me/
// @License Apache 2.0
// @LicenseUrl http://www.apache.org/licenses/LICENSE-2.0.html
package routers

import (
	"captureorderfd/controllers"

	"github.com/astaxie/beego"
)

func init() {
	ns := beego.NewNamespace("/v1",
		beego.NSNamespace("/order",
			beego.NSInclude(
				&controllers.OrderController{},
			),
		),
	)
	beego.AddNamespace(ns)
}
