﻿//Project: TrackingCam (http://TrackingCam.codeplex.com)
//File: UbisenseTrackingPlugin.cs
//Version: 20151127

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;

using Ubisense.Positioning;
using Ubisense.UBase;

namespace TrackingCam.Plugins.Tracking
{
  //MEF
  [Export("Tracking", typeof(ITracker))]
  [Export("Tracking.Ubisense", typeof(ITracker))]
  [ExportMetadata("Description", "Ubisense Tracking")]
  [PartCreationPolicy(CreationPolicy.Shared)]
  public class UbisenseTrackingPlugin : ITracker, IInitializable
  {

    #region --- Fields ---

    protected UbisensePositioning _positioning; //=null
    protected string _key; //=null
    protected double _distance; //=0

    #endregion

    #region --- Initialization ---

    public UbisenseTrackingPlugin()
    {
    }

    public void Initialize(SettingsBase settings) //throws Exception
    {
      _distance = (double?)settings[TrackingSettings.SETTING_TRACKING_DISTANCE] ?? 0; //if set to 0, will use Z value from ubisence as distance at PositionAngle calculation

      _key = (string)settings[TrackingSettings.SETTING_TRACKING_OBJECT_KEY];

      InitializeAsync();
    }

    private void InitializeAsync()
    {
      _positioning = new UbisensePositioning();
      _positioning.GetObjectsCompleted += UbisensePositioning_GetObjectsCompleted;
      _positioning.GetObjectsAsync(); //getting objects may take some time
    }

    #endregion

    #region --- Properties ---

    public double PositionHorizontal
    {
      get
      {
        Position? pos = _positioning.GetPosition();
        return (pos.HasValue) ? pos.Value.P.X : 0;
      }
    }

    public double PositionVertical
    {
      get
      {
        Position? pos = _positioning.GetPosition();
        return (pos.HasValue) ? pos.Value.P.Y : 0;
      }
    }

    public double PositionDepth
    {
      get
      {
        Position? pos = _positioning.GetPosition();
        return (pos.HasValue) ? pos.Value.P.Z : 0;
      }
    }

    public double PositionAngle
    {
      get
      {
        return Math.Atan2(PositionHorizontal, (_distance != 0) ? _distance : PositionDepth); //if a distance hasn't been set, assume the camera is placed at the start of the ubisense area (assuming it is set to z=0), in the middle of it (assuming it is set to x=0)
      }
    }

    #endregion

    #region --- Events ---

    private void UbisensePositioning_GetObjectsCompleted(object sender, SortedDictionary<string, UObject> objects)
    {
      foreach (var o in objects)
        if (o.Key == _key)
          _positioning.SelectedObject = o.Value;
    }

    #endregion

  }

}