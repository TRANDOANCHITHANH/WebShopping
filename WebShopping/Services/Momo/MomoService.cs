using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using WebShopping.Models;
using WebShopping.Models.Momo;

namespace WebShopping.Services.Momo
{
	public class MomoService : IMomoService
	{
		private readonly IOptions<MomoOptionModel> _options;
		public MomoService(IOptions<MomoOptionModel> options)
		{
			_options = options;
		}
		public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
		{
			model.OrderId = DateTime.UtcNow.Ticks.ToString();
			model.OrderInfomation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfomation;
			var rawData =
				$"partnerCode={_options.Value.PartnerCode}" +
				$"&accessKey={_options.Value.AccessKey}" +
				$"&requestId={model.OrderId}" +
				$"&amount={model.Amount}" +
				$"&orderId={model.OrderId}" +
				$"&orderInfo={model.OrderInfomation}" +
				$"&returnUrl={_options.Value.ReturnUrl}" +
				$"&notifyUrl={_options.Value.NotifyUrl}" +
				$"&extraData=";
			var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
			var client = new RestClient(_options.Value.MomoApiUrl);
			var request = new RestRequest() { Method = Method.Post };
			request.AddHeader("Content-Type", "application/json; charset=UTF-8");
			// Create an object representing the request data
			var requestData = new
			{
				partnerCode = _options.Value.PartnerCode,
				accessKey = _options.Value.AccessKey,
				requestType = _options.Value.RequestType,
				amount = model.Amount.ToString(),
				orderId = model.OrderId,
				orderInfo = model.OrderInfomation,
				returnUrl = _options.Value.ReturnUrl,
				notifyUrl = _options.Value.NotifyUrl,
				extraData = "",
				requestId = model.OrderId,

				signature = signature
			};
			request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
			var response = await client.ExecuteAsync(request);
			return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
		}
		public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
		{
			var amount = collection.First(s => s.Key == "amount").Value;
			var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
			var orderId = collection.First(s => s.Key == "orderId").Value;
			return new MomoExecuteResponseModel()
			{
				Amount = amount,
				OrderId = orderId,
				OrderInfo = orderInfo
			};
		}

		public string ComputeHmacSha256(string message, string secretKey)
		{
			var encoding = new UTF8Encoding();
			byte[] keyByte = encoding.GetBytes(secretKey);
			byte[] messageBytes = encoding.GetBytes(message);
			using (var hmacsha256 = new HMACSHA256(keyByte))
			{
				byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);
				return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
			}
		}
	}
}

