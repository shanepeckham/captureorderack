package controllers

import (
	"captureorderfd/models"
	"encoding/json"

	"github.com/astaxie/beego"
)

// Operations about object
type OrderController struct {
	beego.Controller
}

// @Title Capture Order
// @Description Capture order POST
// @Param	body	body 	models.Order true		"body for order content"
// @Success 200 {string} models.Order.ID
// @Failure 403 body is empty
// @router / [post]
func (this *OrderController) Post() {

	var ob models.Order
	json.Unmarshal(this.Ctx.Input.RequestBody, &ob)
	orderID := models.AddOrderToMongoDB(ob)
	this.Data["json"] = map[string]string{"orderId": orderID}
	this.ServeJSON()
}
