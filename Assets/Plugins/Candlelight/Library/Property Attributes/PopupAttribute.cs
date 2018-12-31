﻿// 
// PopupAttribute.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// A custom attribute for specifying that a field should display a popup.
	/// </summary>
	public class PopupAttribute : UnityEngine.PropertyAttribute
	{
		#region Delegates
		/// <summary>
		/// A callback for getting the labels and underlying values for a popup menu. Returns the index of the currently
		/// selected value.
		/// </summary>
		public delegate int GetPopupContentsCallback(List<GUIContent> labels, List<object> values);
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the popup contents for a string backing field that serializes a type.
		/// </summary>
		/// <remarks>This method empties but does not populate <paramref name="values"/> at run-time.</remarks>
		/// <returns>The index of the currently selected type in the list of <paramref name="values"/>.</returns>
		/// <param name="backingField">A <see cref="System.String"/> storing the serialized value.</param>
		/// <param name="typeFilter">A list of all the selectable types in the order they should appear.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="values">Values.</param>
		/// <param name="labelMaker">A method to create label text from a particular <see cref="System.Type"/>.</param>
		public static int GetTypePopupContents(
			string backingField,
			IList<System.Type> typeFilter,
			IList<GUIContent> labels, 
			List<object> values,
			System.Func<System.Type, string> labelMaker = null
		)
		{
			labels.Clear();
			values.Clear();
#if UNITY_EDITOR
			labels.Add(new GUIContent("None"));
			values.Add(string.Empty);
			using (var depths = new ListPool<int>.Scope())
			{
				int maxDepth = 0;
				for (int i = 0; i < typeFilter.Count; ++i)
				{
					int depth = 0;
					System.Type baseType = typeFilter[i];
					while (baseType.BaseType != null)
					{
						baseType = baseType.BaseType;
						if (typeFilter.Contains(baseType))
						{
							++depth;
						}
					}
					depths.List.Add(depth);
					maxDepth = Mathf.Max(maxDepth, depth);
				}
				for (int i = 0; i < typeFilter.Count; ++i)
				{
					string labelText = labelMaker == null ? typeFilter[i].ToString() : labelMaker(typeFilter[i]);
					labelText = string.Format("{0}{1}", new string(' ', depths.List[i] * 2), labelText);
					labels.Add(new GUIContent(labelText));
					values.Add(typeFilter[i].AssemblyQualifiedName);
				}
			}
#endif
			return backingField == null ? 0 : values.IndexOf(backingField);
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="PopupAttribute"/> class.
		/// </summary>
		/// <param name="popupContentsGetter">Name of a <see cref="PopupAttribute.GetPopupContentsCallback"/>.</param>
		/// <param name="statusGetter">Name of a <see cref="StatusPropertyAttribute.GetStatusCallback"/>.</param>
		public PopupAttribute(string popupContentsGetter, string statusGetter = null)
		{
			this.PopupContentsGetter = popupContentsGetter;
			this.StatusGetter = statusGetter;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the name of the <see cref="PopupAttribute.GetPopupContentsCallback"/>.
		/// </summary>
		/// <value>The name of the <see cref="PopupAttribute.GetPopupContentsCallback"/>.</value>
		public string PopupContentsGetter { get; private set; }
		/// <summary>
		/// Gets the name of the <see cref="StatusPropertyAttribute.GetStatusCallback"/>, if any.
		/// </summary>
		/// <value>The name of the <see cref="StatusPropertyAttribute.GetStatusCallback"/>, if any.</value>
		public string StatusGetter { get; private set; }
		#endregion
	}
}