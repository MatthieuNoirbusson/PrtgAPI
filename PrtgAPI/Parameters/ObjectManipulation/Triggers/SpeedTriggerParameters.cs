﻿using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Speed"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class SpeedTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// Gets or sets the <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(nameof(TriggerProperty.OffNotificationAction))]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// Gets or sets the channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Channel))]
        public TriggerChannel Channel
        {
            get { return TriggerChannel.ParseForRequest(GetCustomParameterValue(TriggerProperty.Channel)); }
            set { UpdateCustomParameter(TriggerProperty.Channel, value, true); }
        }

        /// <summary>
        /// Gets or sets the delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Latency))]
        public int? Latency
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Latency); }
            set { UpdateCustomParameter(TriggerProperty.Latency, value); }
        }

        /// <summary>
        /// Gets or sets the condition that controls when the <see cref="Threshold"/> is activated.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Condition))]
        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?) GetCustomParameterEnumInt<TriggerCondition>(TriggerProperty.Condition); }
            set { UpdateCustomParameter(TriggerProperty.Condition, (int?) value, true); }
        }

        /// <summary>
        /// Gets or sets the value which, once reached, will cause this trigger will activate. Used in conjunction with <see cref="Condition"/>.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Threshold))]
        public int? Threshold
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// Gets or sets the time component of the data rate that causes this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.UnitTime))]
        public TimeUnit? UnitTime
        {
            get { return (TimeUnit?) GetCustomParameterEnumXml<TimeUnit>(TriggerProperty.UnitTime); }
            set { UpdateCustomParameter(TriggerProperty.UnitTime, value?.EnumToXml(), true); }
        }

        /// <summary>
        /// Gets or sets the unit component of the data rate that causes this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.UnitSize))]
        public DataUnit? UnitSize
        {
            get { return (DataUnit?) GetCustomParameterEnumXml<DataUnit>(TriggerProperty.UnitSize); }
            set { UpdateCustomParameter(TriggerProperty.UnitSize, value?.EnumToXml(), true); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public SpeedTriggerParameters(int objectId) : base(TriggerType.Speed, objectId, (int?)null, ModifyAction.Add)
        {
            OffNotificationAction = null;
            Channel = TriggerChannel.Primary;
            Condition = TriggerCondition.Above;
            UnitSize = DataUnit.Byte;
            UnitTime = TimeUnit.Hour;
            Threshold = 0;
            Latency = 60;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to. Note: if the trigger is inherited, the ParentId should be specified.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public SpeedTriggerParameters(int objectId, int triggerId) : base(TriggerType.Speed, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Speed"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public SpeedTriggerParameters(int objectId, NotificationTrigger sourceTrigger) : base(TriggerType.Speed, objectId, sourceTrigger, ModifyAction.Add)
        {
            OffNotificationAction = sourceTrigger.OffNotificationAction;
            Channel = sourceTrigger.Channel;
            Latency = sourceTrigger.Latency;
            Condition = sourceTrigger.Condition;
            Threshold = sourceTrigger.ThresholdInternal;
            UnitTime = sourceTrigger.UnitTime;
            UnitSize = sourceTrigger.UnitSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for editing an existing <see cref="TriggerType.Speed"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public SpeedTriggerParameters(NotificationTrigger sourceTrigger) : base(TriggerType.Speed, sourceTrigger.ObjectId, sourceTrigger, ModifyAction.Edit)
        {
        }
    }
}
