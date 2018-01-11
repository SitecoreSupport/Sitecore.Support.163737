using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Cd.EmailEvents;
using Sitecore.EmailCampaign.Cd.Pipelines.RedirectUrl;
using Sitecore.Modules.EmailCampaign.Factories;

namespace Sitecore.Support.EmailCampaign.Cd.Pipelines.RedirectUrl
{
    public class RegisterPageEvents
    {
        private readonly EcmFactory _factory;
        private Regex _ignoredUrlPatternRegex;

        public RegisterPageEvents() : this(EcmFactory.GetDefaultFactory())
        {
        }

        internal RegisterPageEvents(EcmFactory factory)
        {
            Assert.ArgumentNotNull(factory, "factory");
            this._factory = factory;
        }

        public void Process(RedirectUrlPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentCondition(args.RedirectToUrl != null, "args", "RedirectToUrl not set");
            RegistrationResult registrationResult = args.GetRegistrationResult();
            bool flag = (registrationResult != null) && registrationResult.IsDuplicate;
            bool flag2 = (registrationResult != null) && registrationResult.IsFirstRegistration;
            if (!flag)
            {
                if (flag2 && !string.IsNullOrEmpty(this.FirstEventName))
                {
                    this.RegisterPageEvent(args.IsInternalReference, args.RedirectToUrl.ToString(), this.FirstEventName, string.Empty);
                }
                if (!string.IsNullOrEmpty(this.EventName))
                {
                    this.RegisterPageEvent(args.IsInternalReference, args.RedirectToUrl.ToString(), this.EventName, string.Empty);
                }
            }
        }

        private void RegisterPageEvent(bool isInternalReference, string redirectUrl, string eventName, string eventText)
        {
            Assert.ArgumentNotNull(redirectUrl, "redirectUrl");
            Assert.ArgumentNotNull(eventName, "eventName");
            Assert.ArgumentNotNull(eventText, "eventText");
            if (isInternalReference)
            {
                if ((this._ignoredUrlPatternRegex == null) || !this._ignoredUrlPatternRegex.IsMatch(redirectUrl))
                {
                    this._factory.Gateways.AnalyticsGateway.RegisterNextPageEvent(eventName, eventText);
                }
            }
            else
            {
                Tracker.Current.CurrentPage.SetUrl(redirectUrl);
                this._factory.Gateways.AnalyticsGateway.RegisterCurrentPageEvent(eventName, eventText);
            }
        }

        public string EventName { get; set; }

        public string FirstEventName { get; set; }

        public string IgnoredUrlPattern
        {
            set
            {
                this._ignoredUrlPatternRegex = (value == null) ? null : new Regex(value, RegexOptions.Compiled);
            }
        }
    }
}
