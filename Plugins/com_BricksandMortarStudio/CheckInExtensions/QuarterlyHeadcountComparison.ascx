<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuarterlyHeadcountComparison.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.QuarterlyHeadcountComparison" %>

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
                <div class="row" style="margin-bottom: 1em;">
                    <div class="col-md-2">
                        <Rock:RockDropDownList runat="server" ID="ddlCurrentYear" Label="Current Year" AutoPostBack="True" OnSelectedIndexChanged="ddlYear_OnSelectedIndexChanged" />
                    </div>
                    <div class="col-md-2">
                        <Rock:RockDropDownList runat="server" ID="ddlComparisonYear" Label="Comparison Year" AutoPostBack="True" OnSelectedIndexChanged="ddlYear_OnSelectedIndexChanged" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBoxList ID="cblCheckInTemplates" runat="server" Visible="True" RepeatDirection="Horizontal" Label="Attendance Groups" OnSelectedIndexChanged="cblCheckInTemplates_OnSelectedIndexChanged" AutoPostBack="True" />

                    </div>
                    <div class="col-md-4" style="margin-top: 2em">
                        <Rock:BootstrapButton runat="server" ID="bbRefresh" OnClick="bbRefresh_OnClick" CssClass="btn btn-primary btn-sm" ToolTip="Refresh Grid"> <i class="fa fa-refresh"></i></Rock:BootstrapButton>
                        <Rock:HelpBlock runat="server" Text="Q1 = Jan, Feb, Mar; Q2 = Apr, May, Jun; Q3 = Jul, Aug, Sep; Q4 = Oct, Nov, Dec"></Rock:HelpBlock>
                    </div>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gInformation" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Service" HeaderText="Service" />
                            <Rock:RockBoundField DataField="Quarter.Q1Total" HeaderText="Q1" SortExpression="Quarter.Q1Total"  />
                            <Rock:RockBoundField DataField="Quarter.Q2Total" HeaderText="Q2" SortExpression="Quarter.Q2Total" />
                            <Rock:RockBoundField DataField="Quarter.Q3Total" HeaderText="Q3" SortExpression="Quarter.Q3Total" />
                            <Rock:RockBoundField DataField="Quarter.Q4Total" HeaderText="Q4" SortExpression="Quarter.Q4Total" />
                            <Rock:RockBoundField DataField="Quarter.YearTotal" HeaderText="Total" SortExpression="Quarter.Total" />
                            <Rock:RockBoundField DataField="Quarter.Average" HeaderText="Average" SortExpression="Quarter.Average" DataFormatString="{0:0.00}" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.Q1Total" HeaderText="Q1" SortExpression="ComparisonQuarter.Q1Total" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.Q2Total" HeaderText="Q2" SortExpression="ComparisonQuarter.Q2Total" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.Q3Total" HeaderText="Q3" SortExpression="ComparisonQuarter.Q3Total" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.Q4Total" HeaderText="Q4" SortExpression="ComparisonQuarter.Q4Total" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.YearTotal" HeaderText="Total" SortExpression="ComparisonQuarter.Total" />
                            <Rock:RockBoundField DataField="ComparisonQuarter.Average" HeaderText="Average" SortExpression="ComparisonQuarter.Average" DataFormatString="{0:0.00}" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
