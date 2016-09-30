<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FindInactives.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.FindInactives" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Inactive Attendees</h1>
            </div>
            <div class="panel-body" runat="server">
                <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <div class="row">
                    <div class="col-md-12" id="campusContainer" runat="server">
                        <Rock:SlidingDateRangePicker ID="sdrpAttendedBetween" EnabledSlidingDateRangeTypes="Last" runat="server" SlidingDateRangeMode="Last" Label="Attended Since" />
                     </div>
                    <div class="col-md-3" id="refresh" runat="server" >
                        <Rock:BootstrapButton runat="server"  id="btnRefresh" CssClass="btn btn-primary" OnClick="Refresh">Refresh</Rock:BootstrapButton>
                    </div>
                </div>
                <div class="grid grid-panel" style="margin-top: 1em">
                    <Rock:Grid ID="gList" DisplayType="Full" PersonIdField="PersonId" runat="server" AllowSorting="True" AlternatingRowStyle="True" DataKeyNames="Id" OnRowSelected="gClick">
                        <Columns>
                            <Rock:RockBoundField DataField="Fullname" HeaderText="Name" SortExpression="LastName,NickName" HtmlEncode="false" />
                            <Rock:DefinedValueField DataField="ConnectionStatusValue" HeaderText="Connection Status"/>
                            <Rock:DefinedValueField DataField="RecordStatusValue" HeaderText="Record Status"/>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
