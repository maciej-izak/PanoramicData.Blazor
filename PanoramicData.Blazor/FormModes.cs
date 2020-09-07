﻿namespace PanoramicData.Blazor
{
	/// <summary>
	/// An enumeration of possible Form modes.
	/// </summary>
	public enum FormModes
	{
		/// <summary>
		/// No form fields are displayed.
		/// </summary>
		Hidden,
		/// <summary>
		/// The form is being used to define a new entity.
		/// </summary>
		Create,
		/// <summary>
		/// The form is being used to edit an existing entity.
		/// </summary>
		Edit,
		/// <summary>
		/// The form is being used confirm deletion of an existing entity.
		/// </summary>
		Delete
	}
}
