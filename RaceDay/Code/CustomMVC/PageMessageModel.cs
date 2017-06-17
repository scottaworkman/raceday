using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceDay
{
	public class PageMessageModel
	{
		public PageMessageModel()
		{
			Dismiss = MessageDismissEnum.none;
			Timeout = 5000;
			CssClass = CssMessageClassEnum.none;
			Message = String.Empty;
		}

		public PageMessageModel(MessageDismissEnum dismiss, CssMessageClassEnum cssClass, string message)
		{
			Dismiss = dismiss;
			Timeout = 5000;
			CssClass = cssClass;
			Message = message;
		}

		public MessageDismissEnum Dismiss { get; set; }
		public Int32 Timeout { get; set; }
		public CssMessageClassEnum CssClass { get; set; }
		public string Message { get; set; }
		public Exception AppException
		{
			set
			{
				Exception e = value;
				while (e.InnerException != null)
					e = e.InnerException;
				Message = e.Message;
			}
		}

		public override string ToString()
		{
			if (String.IsNullOrEmpty(Message))
				return String.Empty;
			else if (CssClass != CssMessageClassEnum.none)
			{
				if (Dismiss == MessageDismissEnum.none)
					return string.Format("<div class=\"{0}\">{1}</div>", CssClass.GetEnumDescription<CssMessageClassEnum>(), Message);
				else if (Dismiss == MessageDismissEnum.close)
					return string.Format("<div class=\"{0}\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\">&times;</button>{1}</div>", CssClass.GetEnumDescription<CssMessageClassEnum>(), Message);
				else
					return string.Format("<div class=\"{0} alert-timeout\" data-timeout=\"{2}\">{1}</div>", CssClass.GetEnumDescription<CssMessageClassEnum>(), Message, Timeout.ToString());
			}

			return base.ToString();
		}
	}
}