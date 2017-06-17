using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RaceDay
{
	public enum CssMessageClassEnum
	{
		none,
		[Description("text-warning")]
		textwarning,
		[Description("text-error")]
		texterror,
		[Description("text-info")]
		textinfo,
		[Description("text-success")]
		textsuccess,
		[Description("alert alert-block")]
		alertblock,
		[Description("alert alert-block alert-error")]
		errorblock,
		[Description("alert alert-block alert-success")]
		successblock,
		[Description("alert alert-block alert-info")]
		infoblock,
		[Description("alert")]
		alert,
		[Description("alert alert-error")]
		error,
		[Description("alert alert-success")]
		success,
		[Description("alert alert-info")]
		info,
	}
}