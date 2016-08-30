<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FindAttendedClasses.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.FindAttendedClasses" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Attended Groups</h1>
            </div>
            <div class="panel-body" runat="server">
                <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <div class="row">
                    <div class="col-md-12" id="campusContainer" runat="server">
                        <Rock:SlidingDateRangePicker ID="sdrpAttendedBetween" EnabledSlidingDateRangeTypes="Last" runat="server" SlidingDateRangeMode="Last" Label="Classes Attended Since" />
                     </div>
                    <div class="col-md-3" id="refresh" runat="server" >
                        <Rock:BootstrapButton runat="server"  id="btnRefresh" CssClass="btn btn-primary" OnClick="Refresh">Refresh</Rock:BootstrapButton>
                    </div>
                </div>
                <div class="grid grid-panel" style="margin-top: 1em">
                    <Rock:Grid ID="gList" runat="server" PersonIdField="Person.Id" DataKeyNames="Id" AllowSorting="True" AlternatingRowStyle="True" EmptyDataText="No Attendance Found for the Selected Range" OnRowSelected="gClick">
                        <Columns>
                            <Rock:RockBoundField DataField="Person.Fullname" HeaderText="Name" SortExpression="LastName,NickName" />
                            <Rock:RockBoundField DataField="AttendedCount" HeaderText="Number of Times Attended" SortExpression="AttendedCount" />
                            <Rock:DateField DataField="LastAttended" HeaderText="Last Attended" SortExpression="LastAttended" />
                            <Rock:ListDelimitedField DataField="AttendedGroups" HeaderText="Groups Attended" SortExpression="AttendedGroups" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
