using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaceDay
{
	/// <summary>
	/// Class to implement a RequiredIf validation attribute
	/// </summary>
	public class RequiredIf : ValidationAttribute, IClientValidatable
	{
		public String DependentProperty { get; set; }
		public Int32 DependentValue { get; set; }
		public String ClientValidationType { get; set; }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// get a reference to the property this validation depends upon
			var containerType = validationContext.ObjectInstance.GetType();
			var field = containerType.GetProperty(this.DependentProperty);

			if (field != null)
			{
				// get the value of the dependent property
				Int32 dependentvalue = Convert.ToInt32(field.GetValue(validationContext.ObjectInstance, null));

				// if the dependent value equals the attribute value then make sure we have a value
				if ((dependentvalue == this.DependentValue) && ((value == null) || String.IsNullOrEmpty(value.ToString())))
					return new ValidationResult(ErrorMessage ?? "Required");
			}

			return ValidationResult.Success;
		}

		#region IClientValidatable Members

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var rule = new ModelClientValidationRule()
			{
				ErrorMessage = (this.ErrorMessage ?? "Required"),
				ValidationType = (this.ClientValidationType ?? "requiredif"),
			};

			string depProp = BuildDependentPropertyId(metadata, context as ViewContext);

			rule.ValidationParameters.Add("dependentproperty", depProp);
			rule.ValidationParameters.Add("dependentvalue", this.DependentValue);

			yield return rule;
		}

		#endregion

		private string BuildDependentPropertyId(ModelMetadata metadata,
												ViewContext viewContext)
		{
			// build the ID of the property
			string depProp = viewContext.ViewData.TemplateInfo
								.GetFullHtmlFieldId(this.DependentProperty);
			// unfortunately this will have the name of the current field appended 
			// to the beginning,
			// because the TemplateInfo's context has had this fieldname appended 
			// to it. Instead, we
			// want to get the context as though it was one level higher (i.e. 
			// outside the current property,
			// which is the containing object (our Person), and hence the same 
			// level as the dependent property.
			var thisField = metadata.PropertyName + "_";
			if (depProp.StartsWith(thisField))
				// strip it off again
				depProp = depProp.Substring(thisField.Length);
			return depProp;
		}
	}

	/// <summary>
	/// Class to implement a RequiredIfChecked validation attribute
	/// </summary>
	public class RequiredIfChecked : ValidationAttribute, IClientValidatable
	{
		public String DependentProperty { get; set; }
		public Int32 DependentValue { get; set; }
		public String ClientValidationType { get; set; }
		public String DependentClient { get; set; }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// get a reference to the property this validation depends upon
			var containerType = validationContext.ObjectInstance.GetType();
			var field = containerType.GetProperty(this.DependentProperty);

			if (field != null)
			{
				// get the value of the dependent property
				Int32 dependentvalue = Convert.ToInt32(field.GetValue(validationContext.ObjectInstance, null));

				// if the dependent value equals the attribute value then make sure the field has a value
				if ((dependentvalue == this.DependentValue) && ((value == null) || String.IsNullOrEmpty(value.ToString())))
					return new ValidationResult(ErrorMessage ?? "Required");
			}

			return ValidationResult.Success;
		}

		#region IClientValidatable Members

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var rule = new ModelClientValidationRule()
			{
				ErrorMessage = (this.ErrorMessage ?? "Required"),
				ValidationType = (this.ClientValidationType ?? "requiredifchecked"),
			};

			string depProp = BuildDependentPropertyId(metadata, context as ViewContext);

			rule.ValidationParameters.Add("dependentproperty", depProp);

			yield return rule;
		}

		#endregion

		private string BuildDependentPropertyId(ModelMetadata metadata,
												ViewContext viewContext)
		{
			// build the ID of the property
			string depProp = viewContext.ViewData.TemplateInfo
								.GetFullHtmlFieldId(this.DependentClient);
			// unfortunately this will have the name of the current field appended 
			// to the beginning,
			// because the TemplateInfo's context has had this fieldname appended 
			// to it. Instead, we
			// want to get the context as though it was one level higher (i.e. 
			// outside the current property,
			// which is the containing object (our Person), and hence the same 
			// level as the dependent property.
			var thisField = metadata.PropertyName + "_";
			if (depProp.StartsWith(thisField))
				// strip it off again
				depProp = depProp.Substring(thisField.Length);
			return depProp;
		}
	}
}
