using System;
using System.Collections.Generic;


namespace PassingCar.Extensions.Behaviors
{
    public class StarBehavior : Behavior<View>
    {
        private TapGestureRecognizer tapRecognizer;
        private static readonly List<StarBehavior> defaultBehaviors = new List<StarBehavior>();
        private static readonly Dictionary<string, List<StarBehavior>> starGroups = new Dictionary<string, List<StarBehavior>>();

        public static readonly BindableProperty GroupNameProperty =
            BindableProperty.Create("GroupName",
                                    typeof(string),
                                    typeof(StarBehavior),
                                    null,
                                    propertyChanged: OnGroupNameChanged);

        public string GroupName
        {
            set => SetValue(GroupNameProperty, value);
            get => (string)GetValue(GroupNameProperty);
        }

        public static readonly BindableProperty RatingProperty =
           BindableProperty.Create("Rating",
                                   typeof(int),
                                   typeof(StarBehavior), default(int));

        public int Rating
        {
            set => SetValue(RatingProperty, value);
            get => (int)GetValue(RatingProperty);
        }

        private static void OnGroupNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            StarBehavior behavior = (StarBehavior)bindable;
            string oldGroupName = (string)oldValue;
            string newGroupName = (string)newValue;

            // Remove existing behavior from Group
            if (string.IsNullOrEmpty(oldGroupName))
            {
                _ = defaultBehaviors.Remove(behavior);
            }
            else
            {
                List<StarBehavior> behaviors = starGroups[oldGroupName];
                _ = behaviors.Remove(behavior);

                if (behaviors.Count == 0)
                {
                    _ = starGroups.Remove(oldGroupName);
                }
            }

            // Add New Behavior to the group
            if (string.IsNullOrEmpty(newGroupName))
            {
                defaultBehaviors.Add(behavior);
            }
            else
            {
                List<StarBehavior> behaviors;
                if (starGroups.ContainsKey(newGroupName))
                {
                    behaviors = starGroups[newGroupName];
                }
                else
                {
                    behaviors = new List<StarBehavior>();
                    starGroups.Add(newGroupName, behaviors);
                }

                behaviors.Add(behavior);
            }

        }


        public static readonly BindableProperty IsStarredProperty =
            BindableProperty.Create("IsStarred",
                                    typeof(bool),
                                    typeof(StarBehavior),
                                    false,
                                    propertyChanged: OnIsStarredChanged);

        public bool IsStarred
        {
            set => SetValue(IsStarredProperty, value);
            get => (bool)GetValue(IsStarredProperty);
        }

        private static void OnIsStarredChanged(BindableObject bindable, object oldValue, object newValue)
        {
            StarBehavior behavior = (StarBehavior)bindable;

            if ((bool)newValue)
            {
                string groupName = behavior.GroupName;
                List<StarBehavior> behaviors = string.IsNullOrEmpty(groupName) ? defaultBehaviors : starGroups[groupName];
                bool itemReached = false;
                int count = 1, position = 0;
                // all positions to left IsStarred = true and all position to the right is false
                foreach (StarBehavior item in behaviors)
                {
                    if (item != behavior && !itemReached)
                    {
                        item.IsStarred = true;
                    }
                    if (item == behavior)
                    {
                        itemReached = true;
                        item.IsStarred = true;
                        position = count;
                    }
                    if (item != behavior && itemReached)
                    {
                        item.IsStarred = false;
                    }

                    item.Rating = position;
                    count++;
                }

            }


        }

        public StarBehavior()
        {
            defaultBehaviors.Add(this);
        }

        protected override void OnAttachedTo(View view)
        {
            tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += OnTapRecognizerTapped;
            view.GestureRecognizers.Add(tapRecognizer);
        }

        protected override void OnDetachingFrom(View view)
        {
            _ = view.GestureRecognizers.Remove(tapRecognizer);
            tapRecognizer.Tapped -= OnTapRecognizerTapped;
        }

        private void OnTapRecognizerTapped(object sender, EventArgs args)
        {
            //HACK: PropertyChange does not fire, if the value is not changed :-(
            IsStarred = false;
            IsStarred = true;
        }
    }
}
