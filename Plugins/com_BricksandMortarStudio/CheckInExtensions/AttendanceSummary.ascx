<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSummary.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.AttendanceOverview" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Attendance Summary</h1>
            </div>
            <div class="panel-body" runat="server">

                <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-3" id="campusContainer" runat="server">
                        <Rock:SlidingDateRangePicker ID="sdrpAttendedBetween" EnabledSlidingDateRangeTypes="Last, DateRange" runat="server" SlidingDateRangeMode="DateRange" Label="Date Range" OnSelectedDateRangeChanged="sdrpAttendedBetween_OnSelectedDateRangeChanged" />
                      
                    </div>
                    <div class="col-md-3">
                         <Rock:BootstrapButton runat="server" ID="btnRefresh" CssClass="btn btn-primary" OnClick="Refresh">Refresh</Rock:BootstrapButton>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                          <Rock:RockCheckBoxList id="cblCheckInTemplates" runat="server" Visible="True" RepeatDirection="Horizontal" OnTextChanged="cblCheckInTemplates_OnSelectedIndexChanged" />
                    </div>
                </div>
                <div class="grid grid-panel" style="margin-top: 1em">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="True" AlternatingRowStyle="True" EmptyDataText="No Attendance Found for the Selected Range">
                        <Columns>
                            <Rock:DateField DataField="Week.Start" HeaderText="Week Starting" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
