<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HeadcountValueList.ascx.cs" Inherits="Plugins.com_bricksandmortarstudio.CheckInExtensions.HeadcountMetricValueList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfEntityTypeName" runat="server" />
            <asp:HiddenField ID="hfEntityName" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Headcount Metric Values</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <div class="row">
                    <div class="col-md-12">
                        <h4>Pick a Check-In Group</h4>
                    </div>
                    <div class="col-md-3" id="campusContainer" runat="server">
                        <Rock:RockDropDownList ID="ddlCampuses" runat="server" Label="Campus"
                            Help="The campus to record the headcount for." Required="True" AutoPostBack="True" OnSelectedIndexChanged="ddlCampuses_OnSelectedIndexChanged" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlGroups" runat="server" Required="True" Label="Group" Help="The group to record the headcount for." OnSelectedIndexChanged="ddlGroups_OnSelectedIndexChanged" AutoPostBack="True" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <h4>Record Headcount</h4>
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList runat="server" ID="ddlInstanceTwo" Label="Instance" Help="The date and time to record the headcount for." Enabled="False" AutoPostBack="True" />
                    </div>
                    <div class="col-md-3">
                        <Rock:NumberBox runat="server" ID="nbValue" Label="Value" Help="The number of heads counted to record for the given instance." />
                    </div>

                </div>
                <div class="row" style="margin-bottom: 2em">
                    <div class="col-md-3">
                        <Rock:BootstrapButton ID="bbAdd" OnClick="bbAdd_OnClick" CssClass="btn btn-primary" CausesValidation="True" DataLoadingText="Adding" Text="Add" runat="server"></Rock:BootstrapButton>
                    </div>
                </div>

                <asp:HiddenField runat="server" ID="metricId" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfMetricValues" runat="server">
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gMetricValues" runat="server" DataKeyNames="MetricValueId" AllowSorting="true" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:DateField DataField="StartDateTime" HeaderText="Date" SortExpression="StartDateTime" />
                            <Rock:RockBoundField DataField="Group.Name" HeaderText="Group" SortExpression="Group.Name" />
                            <Rock:RockBoundField DataField="Headcount" HeaderText="Headcount" SortExpression="Headcount" />
                            <Rock:RockBoundField DataField="CheckInCount" HeaderText="Check-In Count" SortExpression="CheckInCount" />
                            <Rock:DeleteField OnClick="gMetricValues_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

