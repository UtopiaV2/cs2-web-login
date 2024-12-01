namespace cs2_web_login;

public static class Rel
{
  public static Release CreateNull() => new(
      Url: "",
        AssetsUrl: "",
      UploadUrl: "",
      HtmlUrl: "",
      Id: 0,
      Author: new Author(
          Login: "",
          Id: 0,
          NodeId: "",
          AvatarUrl: "",
          GravatarId: "",
          Url: "",
          HtmlUrl: "",
          FollowersUrl: "",
          FollowingUrl: "",
          GistsUrl: "",
          StarredUrl: "",
          SubscriptionsUrl: "",
          OrganizationsUrl: "",
          ReposUrl: "",
          EventsUrl: "",
          ReceivedEventsUrl: "",
          Type: "",
          UserViewType: "",
          SiteAdmin: false
      ),
      NodeId: "",
      TagName: "",
      TargetCommitish: "",
      Name: "",
      Draft: false,
      Prerelease: false,
      CreatedAt: new(),
      PublishedAt: new(),
      Assets: new List<Asset>(),
      TarballUrl: "",
      ZipballUrl: "",
      Body: ""
    );
}
/*Vim: set expandtab tabstop=2 shiftwidth=2:*/
