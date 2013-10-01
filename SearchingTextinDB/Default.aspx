<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="SearchingTextinDB._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script type ="text/javascript" >
$(document).ready(function() {
 $('#clsAdvance').hide();   
    $('#ddl1').on("change", function() {
		if($(this).val()=="1"){
             $('#td-Methods').hide();
			 $('#td-industry').show();
			 $('#td-customer').show();
        }
        else
        {
          $('#td-Methods').show();
        }
    });
	
</script>

</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div style="vertical-align:middle; text-align: left;">
<div style="margin-bottom:20px; background-color:#D8D1CA;padding:15px;">
    First Click to Index: &nbsp;&nbsp;<asp:Button ID="btnCreateIndex" BackColor="#FF6600" ForeColor="White" runat="server" Text="createIndex" OnClick="btnCreateIndex_Click" />
    <br />
    </div>

    <div style="margin-bottom:20px; background-color:#D8D1CA; padding:15px;">
      <asp:LinkButton ID="lnkbtnSimpleSearch" runat="server" 
        onclick="lnkbtnSimpleSearch_Click">Simple Search</asp:LinkButton>
    <div   id ="idSimple" runat ="server" >
  
    Enter the text to search &nbsp;&nbsp;  <asp:TextBox ID="txtSearchval" runat="server">
    </asp:TextBox>
 <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" BackColor="#FF6600" ForeColor="White" /> 
    </div>
    </div>
   <br />
   <br />
   <div style="margin-bottom:20px; background-color:#D8D1CA; padding:15px;">
        <asp:LinkButton ID="lnbtnAdvance" runat="server" 
        onclick="lnbtnAdvance_Click">Advance Search</asp:LinkButton>
   <br />
   <br />

    <div id ="idAdvance" runat ="server" >

      <table>
    <tr>
    <td>
        <asp:Label ID="lblFindResult" runat="server" Text="Find Results" 
            Font-Bold="True"></asp:Label>
    </td>
    </tr>
    <tr>
    <td>
       with <strong>all</strong> of the words
    </td>
    <td>
        <asp:TextBox ID="txtWords" runat="server"></asp:TextBox>
    </td>
    <td>
        <asp:DropDownList ID="ddlWords" runat="server">
            <asp:ListItem>10 Results</asp:ListItem>
            <asp:ListItem>20 Results</asp:ListItem>
            <asp:ListItem>30 Results</asp:ListItem>
            <asp:ListItem>50 Results</asp:ListItem>
            <asp:ListItem>100 Results</asp:ListItem>
        </asp:DropDownList>
    </td>
    <td>
        <asp:Button ID="btnWordSearch" runat="server" Text="Search >" 
            BackColor="#FF6600" ForeColor="White" onclick="btnWordSearch_Click" />
    </td>
    </tr>
    <tr>
    <td>
        with the <strong>exact phrase</strong>
    </td>
    <td>
        <asp:TextBox ID="txtPhrase" runat="server" 
            ></asp:TextBox>
    </td>
    </tr>
     <tr>
    <td>
        with <strong>at least one</strong> of the words
    </td>
    <td>
        <asp:TextBox ID="txtLeastWords" runat="server"></asp:TextBox>
    </td>
    </tr>
     <tr>
    <td>
        <strong>without</strong> the words
    </td>
    <td>
        <asp:TextBox ID="txtWithoutWords" runat="server"></asp:TextBox>
    </td>
    </tr>
    </table>
    </div>
    </div>
<br />
<br />
<div >
<div style="text-align:center">
<asp:GridView ID="GridView1" runat="server" EmptyDataText="No data found..........">
</asp:GridView>
</div>
</div>
</div>
</asp:Content>
