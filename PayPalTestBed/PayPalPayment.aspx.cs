using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PayPal.Api;
using PayPalTestBed.Utilities;
namespace PayPalTestBed
{
    public partial class PayPalPayment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RunSample();
        }

        protected void RunSample()
        {
            //APIContext
            var apiContext = Configuration.GetAPIContext();

            string payerID = Request.Params["PayerID"];
            if (string.IsNullOrEmpty(payerID))
            {
                //Items
                var itemList = new ItemList()
                {
                    items = new List<Item>()
                    {
                        new Item()
                        {
                            name = "Item Name",
                            currency = "USD",
                            price = "15",
                            quantity = "5",
                            sku = "sku"
                        }
                    }
                };
                //Payer
                var payer = new Payer() { payment_method = "paypal" };
                //Redirect URLs
                var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/PayPalPayment.aspx?";
                var guid = Convert.ToString((new Random()).Next(10000));
                var redirectUrl = baseURI + "guid=" + guid;
                var redirUrls = new RedirectUrls()
                {
                    cancel_url = redirectUrl + "&cancel=true",
                    return_url = redirectUrl
                };
                //Details
                var details = new Details()
                {
                    tax = "15",
                    shipping = "10",
                    subtotal = "75"
                };
                //Amount
                var amount = new Amount()
                {
                    currency = "USD",
                    total = "100.00", //total must be equal to the sum of shipping, tax, and subtotal.
                    details = details
                };
                //Transaction
                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {

                    description = "Transaction Description.",
                    invoice_number = Common.GetRandomInvoiceNumber(),
                    amount = amount,
                    item_list = itemList
                });
                //Payment
                var payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirUrls
                };
                //Create a payment using a valid APIContext
                var createdPayment = payment.Create(apiContext);
                //using the links provided by te createdPayment obbject, 
                //we can give the user the option to redirect to PayPal to approve the payment
                var links = createdPayment.links.GetEnumerator();
                string paypalRedirectUrl = null;
                while (links.MoveNext())
                {
                    var link = links.Current;
                    if (link.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        paypalRedirectUrl = link.href;
                    }
                }
                Session.Add(guid, createdPayment.id);
                Response.Redirect(paypalRedirectUrl, true);
            }
            else
            {
                var guid = Request.Params["guid"];
                //setup payment to execute
                var paymentId = Session[guid] as string;
                var paymentExecution = new PaymentExecution() { payer_id = payerID };
                var payment = new Payment() { id = paymentId };
                var executePayment = payment.Execute(apiContext, paymentExecution);
            }
          
        }
    }
}