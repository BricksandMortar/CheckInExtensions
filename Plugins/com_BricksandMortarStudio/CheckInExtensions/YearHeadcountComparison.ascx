<%@ Control Language="C#" AutoEventWireup="true" CodeFile="YearHeadcountComparison.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.YearOverviewHeadcountComparison" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bar-chart"></i>
                    <asp:Literal ID="lBlockName" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-2">
                        <Rock:RockCheckBox runat="server" AutoPostBack="True" ID="cbDateRangeEnabled" Checked="False" OnCheckedChanged="cbDateRangeEnabled_OnCheckedChanged" Label="Filter by Date Range" />
                    </div>
                    <div class="col-md-3">
                        <Rock:MonthDayPicker runat="server" ID="mdStart" Enabled="False" Required="True" Label="Date Range Start" OnSelectedMonthDayChanged="mdStart_OnSelectedMonthDayChanged" />
                    </div>
                    <div class="col-md-3">

                        <Rock:MonthDayPicker runat="server" ID="mdEnd" Enabled="False" Required="True" Label="Date Range End" OnSelectedMonthDayChanged="mdEnd_OnSelectedMonthDayChanged" />
                    </div>
                </div>
                <div class="row" style="margin-bottom: 1em">
                    <div class="col-md-2">
                        <Rock:RockDropDownList runat="server" ID="ddlCurrentYear" Label="Current Year" OnSelectedIndexChanged="ddlYear_OnSelectedIndexChanged"  AutoPostBack="True"/>
                    </div>
                    <div class="col-md-2">
                        <Rock:RockDropDownList runat="server" ID="ddlComparisonYear" Label="Comparison Year" OnSelectedIndexChanged="ddlYear_OnSelectedIndexChanged"  AutoPostBack="True"/>
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBoxList ID="cblCheckInTemplates" runat="server" Visible="True" RepeatDirection="Horizontal" Label="Attendance Groups" OnSelectedIndexChanged="cblCheckInTemplates_OnSelectedIndexChanged" AutoPostBack="True" />

                    </div>
                    <div class="col-md-4" style="margin-top: 2em">
                        <Rock:BootstrapButton runat="server" ID="bbRefresh" OnClick="bbRefresh_OnClick" CssClass="btn btn-primary btn-sm" ToolTip="Refresh Grid"> <i class="fa fa-refresh"></i></Rock:BootstrapButton></div>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gStatistics" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Service" HeaderText="Service" />
                            <Rock:RockBoundField DataField="TotalMonthly" HeaderText="Month's Total Attendance" SortExpression="TotalMonthly" />
                            <Rock:RockBoundField DataField="AverageMonthlyAttendance" HeaderText="Average Attendance For Month" SortExpression="AverageMonthlyAttendance" DataFormatString="{0:0.00}" />
                            <Rock:RockBoundField DataField="YearToDateTotal" HeaderText="Year to Date Total" SortExpression="YearToDateTotal" />
                            <Rock:RockBoundField DataField="YearToDateAverage" HeaderText="Year to Date Average" SortExpression="YearToDateAverage" DataFormatString="{0:0.00}" />
                            <Rock:RockBoundField DataField="ComparisonYearTotalMonthly" HeaderText="Comparison Year Month's Total Attendance" SortExpression="ComparisonYearTotalMonthly" />
                            <Rock:RockBoundField DataField="ComparisonYearAverageMonthlyAttendance" HeaderText="Comparison Year Average Attendance For Month" SortExpression="ComparisonYearAverageMonthlyAttendance" DataFormatString="{0:0.00}" />
                            <Rock:RockBoundField DataField="ComparisonYearToDateTotal" HeaderText="Comparison Year to Date Total" SortExpression="ComparisonYearToDateTotal" />
                            <Rock:RockBoundField DataField="ComparisonYearToDateAverage" HeaderText="Comparison Year to Date Average" SortExpression="ComparisonYearToDateAverage" DataFormatString="{0:0.00}" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
