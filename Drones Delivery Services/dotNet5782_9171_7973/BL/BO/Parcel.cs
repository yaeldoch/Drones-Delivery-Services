﻿using System;
using StringUtilities;

namespace BO
{
    /// <summary>
    /// A class to represent a PDS of parcel
    /// </summary>
    public class Parcel 
    {
        /// <summary>
        /// Parcel Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Parcel sender
        /// </summary>
        public CustomerInDelivery Sender { get; set; }

        /// <summary>
        /// Parcel reciever
        /// </summary>
        public CustomerInDelivery Target { get; set; }

        WeightCategory weight;
        /// <summary>
        /// Parcel weight category
        /// </summary>
        public WeightCategory Weight
        {
            get => weight;
            set
            {
                if (!Validation.IsValidEnumOption<WeightCategory>((int)value))
                {
                    throw new InvalidPropertyValueException(value);
                }
                weight = value;
            }
        }

        Priority priority;
        /// <summary>
        /// Parcel priority
        /// </summary>
        public Priority Priority
        {
            get => priority;
            set
            {
                if (!Validation.IsValidEnumOption<Priority>((int)value))
                {
                    throw new InvalidPropertyValueException(value);
                }
                priority = value;
            }
        }

        /// <summary>
        /// Drone which delivers parcel
        /// </summary>
        public Drone Drone { get; set; }

        /// <summary>
        /// Parcel requested time (If has already occurred)
        /// </summary>
        public DateTime? Requested { get; set; }

        /// <summary>
        /// Parcel scheduled time, the time the parcel was associated with drone
        /// (If has already occurred)
        /// </summary>
        public DateTime? Scheduled { get; set; }

        /// <summary>
        /// Parcel picked up time, the time the parcel was picked up by a drone
        /// (If has already occurred)
        /// </summary>
        public DateTime? PickedUp { get; set; }

        /// <summary>
        /// Parcel supplied time, the time the parcel was supplied to the target customer up
        /// (If has already occurred)
        /// </summary>
        public DateTime? Supplied { get; set; }


        /// <summary>
        /// Uses an outer project <see cref="StringUtilities"/>
        /// to override the <c>ToString()</c> method
        /// </summary>
        /// <returns>String representation of customer</returns>
        public override string ToString() => this.ToStringProperties();
    }
}
