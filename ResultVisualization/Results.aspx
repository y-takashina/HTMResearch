<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Results.aspx.cs" Inherits="ResultVisualization.Results" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <title>Document</title>
    <style type="text/css">
        #file { width: 209px; }
    </style>
</head>

<body bgcolor="#fff3b8">

<h1>HTM Temporal Pooling</h1>
<form runat="server">
    <asp:DropDownList id="dropDownList" runat="server" OnSelectedIndexChanged="dropDownList_SelectedIndexChanged"/>
</form>

<h2>Layer 1</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <th>membership</th>
    <tr>
        <td>
            <img src="<%= DataPath %>layer1_transitions.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer1_probabilities.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer1_distances_mean.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer1_distances_min.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer12_membership.png" height="200">
        </td>
    </tr>
</table>

<h2>Layer 2</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <th>membership</th>
    <tr>
        <td></td>
        <td>
            <img src="<%= DataPath %>layer2_probabilities.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer2_distances_mean.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer2_distances_min.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer23_membership.png" height="200">
        </td>
    </tr>
</table>

<h2>Layer 3</h2>
<table>
    <th>transitions</th>
    <th>probabilities</th>
    <th>distances_mean</th>
    <th>distances_min</th>
    <tr>
        <td></td>
        <td>
            <img src="<%= DataPath %>layer3_probabilities.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer3_distances_mean.png" width="200">
        </td>
        <td>
            <img src="<%= DataPath %>layer3_distances_min.png" width="200">
        </td>
    </tr>
</table>

<h2>Results</h2>
<h3>Series</h3>
<img src="<%= DataPath %>original.png" width="640">
<h3>Error(htm)</h3>
<img src="<%= DataPath %>error.png" width="640">
<h3>Error(frequency)</h3>
<img src="<%= DataPath %>error2.png" width="640">
</body>