namespace Sitecore.Support.EmailCampaign.Cd.Pipelines.RedirectUrl
{
  using Sitecore.Analytics;
  using System.Collections;
  using System.Text.RegularExpressions;
  using Newtonsoft.Json;
  using Sitecore.Diagnostics;
  using Sitecore.EmailCampaign.Cd.EmailEvents;
  using Sitecore.EmailCampaign.Cd.Pipelines.RedirectUrl;
  using Sitecore.Modules.EmailCampaign.Factories;

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

    private string GetData(RedirectUrlPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Hashtable hashtable = new Hashtable {
                {
                    "MessageId",
                    args.MessageId
                }
            };
      return JsonConvert.SerializeObject(hashtable);
    }

    public void Process(RedirectUrlPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.IsNotNull(args.RedirectToUrl, "RedirectToUrl not set");
      Assert.IsNotNull(args.MessageItem, "MessageItem not set");
      if (args.RegisterEvents && !args.MessageItem.ExcludeFromReports)
      {
        RegistrationResult registrationResult = args.GetRegistrationResult();
        bool flag = (registrationResult != null) && registrationResult.IsDuplicate;
        bool flag2 = (registrationResult != null) && registrationResult.IsFirstRegistration;
        if (!flag)
        {
          if (flag2)
          {
            this.RegisterClickEvent(args, "First Click Email Link");
          }
          this.RegisterClickEvent(args, "Click Email Link");
        }
      }
    }

    private void RegisterClickEvent(RedirectUrlPipelineArgs args, string eventName)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.ArgumentNotNull(eventName, "eventName");
      Sitecore.Analytics.Model.PageEventData data = new Sitecore.Analytics.Model.PageEventData
      {
        Name = eventName,
        Text = string.Empty,
        Data = this.GetData(args)
      };
      if (args.IsInternalReference && this.ShouldRegisterInternalLink(args.RedirectToUrl.ToString()))
      {
        Sitecore.Analytics.Data.PageEventData pageEventData = new Sitecore.Analytics.Data.PageEventData(data);
        this._factory.Gateways.AnalyticsGateway.RegisterNextPageEvent(pageEventData);
      }
      else if (!args.IsInternalReference)
      {
        Tracker.Current.CurrentPage.SetUrl(args.RedirectToUrl.ToString());
        Sitecore.Analytics.Data.PageEventData pageEventData = new Sitecore.Analytics.Data.PageEventData(data);
        this._factory.Gateways.AnalyticsGateway.RegisterCurrentPageEvent(pageEventData);
      }
    }

    private bool ShouldRegisterInternalLink(string redirectUrl)
    {
      Assert.ArgumentNotNull(redirectUrl, "redirectUrl");
      if (this._ignoredUrlPatternRegex != null)
      {
        return !this._ignoredUrlPatternRegex.IsMatch(redirectUrl);
      }
      return true;
    }

    public string IgnoredUrlPattern
    {
      set
      {
        this._ignoredUrlPatternRegex = (value == null) ? null : new Regex(value, RegexOptions.Compiled);
      }
    }
  }
}
