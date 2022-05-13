﻿using System;
using StringUtilities;

namespace BO
{
    /// <summary>
    /// A class to represent a PDS of drone in delivery
    /// </summary>
    public class DroneInDelivery : ILocalable
    {
        /// <summary>
        /// Drone Id
        /// </summary>
        public int Id { get; set; }

        double battery;
        /// <summary>
        /// Drone battery state 
        /// (in parcents)
        /// </summary>
        public double Battery
        {
            get => battery;
            set
            {
                if (!Validation.IsValidBattery(value))
                {
                    throw new InvalidPropertyValueException(value);
                }
                battery = value;
            }
        }

        /// <summary>
        /// Drone location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Uses an outer project <see cref="StringUtilities"/>
        /// to override the <c>ToString()</c> method
        /// </summary>
        /// <returns>String representation of customer</returns>
        public override string ToString() => this.ToStringProperties();
    }
}
