'use strict';
//let getErrorSpan = function (dataValmsgFor) {
//    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
//        "\" data-valmsg-replace=\"true\"></span>";
//}
//let initializeVilidationTr = function (tr) {
//    let fields = tr.find('.field-ownership-right');
//    let idOwnershipRight = tr.data('idownershipright');
//    //Номер
//    let Number = 'Number_' + idOwnershipRight;
//    $(fields[0]).addClass('valid');
//    $(fields[0]).attr('data-val', 'true');
//    $(fields[0]).attr('data-val-required', 'Поле "Номер" является обязательным');
//    $(fields[0]).attr('id', Number);
//    $(fields[0]).attr('name', Number);
//    $(fields[0]).attr('aria-describedby', Number + '-error');
//    $(fields[0]).after(getErrorSpan(Number));
//    //Дата
//    let Date = 'Date_' + idOwnershipRight;
//    $(fields[1]).addClass('valid');
//    $(fields[1]).attr('data-val', 'true');
//    $(fields[1]).attr('data-val-required', 'Поле "Дата" является обязательным');
//    $(fields[1]).attr('id', Date);
//    $(fields[1]).attr('name', Date);
//    $(fields[1]).attr('aria-describedby', Date + '-error');
//    $(fields[1]).after(getErrorSpan(Date));
//    //Тип ограничения
//    let OwnershipRightType = 'IdOwnershipRightType_' + idOwnershipRight;
//    $(fields[3]).addClass('valid');
//    $(fields[3]).attr('data-val', 'true');
//    $(fields[3]).attr('data-val-required', 'Поле "Тип" является обязательным');
//    $(fields[3]).attr('id', OwnershipRightType);
//    $(fields[3]).attr('name', OwnershipRightType);
//    $(fields[3]).attr('aria-describedby', OwnershipRightType + '-error');
//    $(fields[3]).after(getErrorSpan(OwnershipRightType));

//    refreshValidationOwnershipRightsForm();
//}
//let refreshValidationOwnershipRightsForm = function () {
//    var form = $("#ownershipRightsForm")
//        .removeData("validator")
//        .removeData("unobtrusiveValidation");
//    $.validator.unobtrusive.parse(form);
//    form.validate();
//}
//let initializeVilidationTrs = function () {
//    let trs = $('#ownershipRights>tr');
//    trs.each(function () {
//        initializeVilidationTr($(this));
//    });
//}
//let refreshOwnershipRight = function (tr, ownershipRight) {
//    let fields = tr.find('.field-ownership-right');
//    //Номер
//    $(fields[0]).prop('value', ownershipRight.number);
//    $(fields[0]).prop('title', ownershipRight.number);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[0]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Дата
//    $(fields[1]).prop('value', ownershipRight.date);
//    $(fields[1]).prop('title', ownershipRight.date);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[1]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Наименование
//    $(fields[2]).prop('value', ownershipRight.description);
//    $(fields[2]).prop('title', ownershipRight.description);
//    //Тип ограничения
//    $(fields[3]).prop('value', ownershipRight.idOwnershipRightType);
//    $(fields[3]).prop('title', ownershipRight.idOwnershipRightType);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[3]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Планируемая дата переселения
//    $(fields[4]).prop('value', ownershipRight.resettlePlanDate);
//    $(fields[4]).prop('title', ownershipRight.resettlePlanDate);
//    //Планируемая дата сноса
//    $(fields[5]).prop('value', ownershipRight.demolishPlanDate);
//    $(fields[5]).prop('title', ownershipRight.demolishPlanDate);
//}
let BuildingDemolitionActFile = class {
    constructor(idBuilding) {
        this.id = "";
        this.idBuilding = idBuilding;
        this.idActFile = "";
        this.idActTypeDocument = 0;
        this.number = "";
        this.date = "";
        this.name = "";
        this.originalNameActFile = "";
        this.file = "";
    }

    get id() {
        return this._id;
    }
    set id(value) {
        this._id = value;
    }

    get idBuilding() {
        return this._idBuilding;
    }
    set idBuilding(value) {
        this._idBuilding = value;
    }

    get idActFile() {
        return this._idActFile;
    }
    set idActFile(value) {
        this._idActFile = value;
    }

    get idActTypeDocument() {
        return this._idActTypeDocument;
    }
    set idActTypeDocument(value) {
        this._idActTypeDocument = value;
    }

    get number() {
        return this._number;
    }
    set number(value) {
        this._number = value;
    }

    get date() {
        return this._date;
    }
    set date(value) {
        this._date = value;
    }

    get name() {
        return this._name;
    }
    set name(value) {
        this._name = value;
    }

    get originalNameActFile() {
        return this._originalNameActFile;
    }
    set originalNameActFile(value) {
        this._originalNameActFile = value;
    }

    get file() {
        return this._file;
    }
    set file(value) {
        this._file = value;
    }

    initialize = function (tr) {
        let fields = tr.find('.field-building-demolition-act-file');
        this.id = tr.data('idbuildingdemolitionactfile');
        this.idActFile = tr.find('.act-file-block').data('id');
        this.idActTypeDocument = +fields[3].value;
        this.number = fields[0].value;
        this.date = fields[1].value;
        this.name = fields[2].value;
        this.originalNameActFile = tr.find('.act-file').attr('title');

        let fileInput = tr.find('.act-file-upload')[0];
        if (fileInput.files[0] != undefined) {
            this.file = fileInput.files[0];
        }
        else {
            this.file = "";
        }
    }

    setAllProperty = function (jsonBuildingDemolitionActFiles) {
        this.id = jsonBuildingDemolitionActFiles.id;
        this.idBuilding = jsonBuildingDemolitionActFiles.idBuilding;
        this.idActFile = jsonBuildingDemolitionActFiles.idActFile;
        this.idActTypeDocument = jsonBuildingDemolitionActFiles.idActTypeDocument;
        this.number = jsonBuildingDemolitionActFiles.number == null ? "" : jsonBuildingDemolitionActFiles.number;
        this.date = jsonBuildingDemolitionActFiles.date;
        this.name = jsonBuildingDemolitionActFiles.name == null ? "" : jsonBuildingDemolitionActFiles.name;
        this.originalNameActFile = jsonBuildingDemolitionActFiles.originalNameActFile;
        this.file = ""
    }
}

let BuildingDemolitionInfo = class {
    constructor(root) {
        this._root = root;
        this._action = root.data('action');
        this.idBuilding = +this._root.data('idbuilding');
    }

    get idBuilding() {
        return this._idBuilding;
    }
    set idBuilding(value) {
        this._idBuilding = value;
    }

    get idBuilding() {
        return this._idBuilding;
    }
    set idBuilding(value) {
        this._idBuilding = value;
    }

    get demolishPlanDate() {
        return this._demolishPlanDate;
    }
    set demolishPlanDate(value) {
        this._demolishPlanDate = value;
    }

    get buildingDemolitionActFiles() {
        return this._buildingDemolitionActFiles;
    }
    set buildingDemolitionActFiles(value) {
        this._buildingDemolitionActFiles = value;
    }

    get actTypeDocuments() {
        return this._actTypeDocuments;
    }
    set actTypeDocuments(value) {
        this._actTypeDocuments = value;
    }

    initialize = function () {
        this._root = $('#buildingDemolitionInfoForm');
        this._action = this._root.data('action');
        this.idBuilding = +this._root.data('idbuilding');
        this.demolishPlanDate = this._root.find('#demolishPlanDate').val();
        this.buildingDemolitionActFiles = [];

        let trs = this._root.find('#buildingDemolitionActFiles>tr');
        for (let i = 0; i < trs.length; i++) {
            this.buildingDemolitionActFiles[i] = new BuildingDemolitionActFile(this.idBuilding);
            this.buildingDemolitionActFiles[i].initialize($(trs[i]));
        }
    }

    setAllProperty = function (jsonBuildingDemolitionActFiles) {
        this.idBuilding = jsonBuildingDemolitionActFiles.idBuilding;
        this.demolishPlanDate = jsonBuildingDemolitionActFiles.demolishPlanDate;
        this.actTypeDocuments = jsonBuildingDemolitionActFiles.actTypeDocuments;
        this.buildingDemolitionActFiles = [];
        for (let i = 0; i < jsonBuildingDemolitionActFiles.buildingDemolitionActFiles.length; i++) {
            this.buildingDemolitionActFiles[i] = new BuildingDemolitionActFile(this.idBuilding);
            this.buildingDemolitionActFiles[i].setAllProperty(jsonBuildingDemolitionActFiles.buildingDemolitionActFiles[i]);
        }
    }

    updateData = function () {
        let obj = this;
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/BuildingDemolitionInfo/GetBuildingDemolitionInfo',
            data: { idBuilding: this.idBuilding },
            dataType: 'json',
            success: function (buildingDemolitionInfo) {
                obj.setAllProperty(buildingDemolitionInfo);
            }
        });
    }

    getFormData = function () {
        this.initialize();
        let buildingDemolitionInfoVM = new FormData();
        buildingDemolitionInfoVM.append('IdBuilding', this.idBuilding);
        buildingDemolitionInfoVM.append('DemolishPlanDate', this.demolishPlanDate);
        for (var i = 0; i < this.buildingDemolitionActFiles.length; i++) {
            if (this.buildingDemolitionActFiles[i].file != "") {
                buildingDemolitionInfoVM.append('Files', this.buildingDemolitionActFiles[i].file, this.buildingDemolitionActFiles[i].file.name);
            }
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].Id', this.buildingDemolitionActFiles[i].id);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].IdBuilding', this.idBuilding);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].IdActFile', this.buildingDemolitionActFiles[i].idActFile);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].IdActTypeDocument', this.buildingDemolitionActFiles[i].idActTypeDocument);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].Number', this.buildingDemolitionActFiles[i].number);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].Date', this.buildingDemolitionActFiles[i].date);
            buildingDemolitionInfoVM.append('BuildingDemolitionActFiles[' + i + '].Name', this.buildingDemolitionActFiles[i].name);
        };
        return buildingDemolitionInfoVM;
    }


    createActTypeDocumentSelect = function (selectedId) {
        let select = `<select class="form-control field-building-demolition-act-file" title="">\n`;
        let options = "";
        for (let i = 0; i < this.actTypeDocuments.length; i++) {
            options += `<option value="${this.actTypeDocuments[i].id}"`;
            if (this.actTypeDocuments[i].id == selectedId) {
                select = `<select class="form-control field-building-demolition-act-file" title="${this.actTypeDocuments[i].name}">\n`;
                options += ` selected`;
            }
            options += `>${this.actTypeDocuments[i].name}</option>\n`;
        }
        select += options + `</select>\n`;
        return select;
    }

    addNewTr = function () {
        let obj = new BuildingDemolitionActFile(this.idBuilding);
        this._root.find('#buildingDemolitionActFiles').append(this.createTr(obj));        
        this.drawActFiles(this._root.find('#buildingDemolitionActFiles>tr').last());
    }

    createTr = function (buildingDemolitionActFile) {
        let originalName = "";
        let idFile = ""; //значение null для пустого файла
        let id = "actFile_" + guid();
        if (buildingDemolitionActFile.idActFile != "") {
            idFile = buildingDemolitionActFile.idActFile;
            id = "actFile_" + idFile;
            originalName = buildingDemolitionActFile.originalNameActFile;
        }
        buildingDemolitionActFile.idActFile == null ? "" : buildingDemolitionActFile.idActFile;
        let tr = `
        <tr class="building-demolition-act-file" data-idbuildingdemolitionactfile="${buildingDemolitionActFile.id}">
            <td class="align-middle">
                <input type="text" class="form-control field-building-demolition-act-file" value="${buildingDemolitionActFile.number}" title="${buildingDemolitionActFile.number}">
            </td>
            <td class="align-middle">
                <input type="date" class="form-control field-building-demolition-act-file" value="${buildingDemolitionActFile.date}" title="${buildingDemolitionActFile.date}">
            </td>
            <td class="align-middle"><input type="text" class="form-control field-building-demolition-act-file" value="${buildingDemolitionActFile.name}" title="${buildingDemolitionActFile.name}"></td>
            <td class="align-middle">
        `
        + this.createActTypeDocumentSelect(buildingDemolitionActFile.idActTypeDocument) +
        `    </td>
            <td>
                <div class="act-file-block" data-id="${idFile}">
                    <input type="file" id="${id}" class="act-file-upload"/>
                    <label for="${id}" class="btn btn-success oi oi-paperclip act-file-add"></label>
                    <div class="act-file" title="${originalName}" style="cursor: pointer;">
                        <a href="#" class="btn-link act-file-link">${originalName}</a>
                        <span class="badge btn btn-danger ml-1">&times;</span>
                    </div>
                </div>
            </td>
            <td class="align-middle">
                <a href="#" class="btn btn-danger oi oi-x" title="Удалить" aria-label="Удалить"></a>
            </td>
        </tr>
        `;
        return tr;
    }

    updateDataInElements = function () {
        this._root.data('idbuilding', this.idBuilding);
        this._root.attr('data-idbuilding', this.idBuilding);

        this._root.find('#demolishPlanDate').val(this.demolishPlanDate);

        this._root.find('#buildingDemolitionActFiles>tr').remove();
        if (this.buildingDemolitionActFiles.length == 0) return;
        for (let i = 0; i < this.buildingDemolitionActFiles.length; i++) {
            this._root.find('#buildingDemolitionActFiles').append(this.createTr(this.buildingDemolitionActFiles[i]));
        }
        this.drawActFiles();
    }

    drawActFiles = function (trs = this._root.find('#buildingDemolitionActFiles>tr')) {
        let files = trs.find($('.act-file-block'));
        trs.find($('.act-file-upload')).hide();
        files.each(function () {
            if ($(this).data('id') > 0) {
                $(this).find('.act-file-add').hide();
            }
            else {
                $(this).find('.act-file').hide();
            }
        });
    }
}

let buildingDemolitionActFileAddClick = function (event) {
    event.preventDefault();
    _buildingDemolitionInfo.addNewTr();
}
let buildingDemolitionInfoSaveClick = function (event) {
    event.preventDefault();
    if ($('#buildingDemolitionInfoForm').valid()) {
        let formData = _buildingDemolitionInfo.getFormData();
        let xhr = new XMLHttpRequest();
        xhr.open("POST", window.location.origin + '/BuildingDemolitionInfo/SaveBuildingDemolitionInfo');
        xhr.send(formData)
        xhr.onreadystatechange = function () {
            if (this.readyState != 4) return;
            console.log(xhr);
            console.log(xhr.status);
            if (xhr.status == 200 && xhr.responseText == 1) {
                alert('Все гуд');
            } else {
                alert("Ошибка сохранения информации о сносе!" +
                    "\nxhrresponseText: " + xhr.responseText +
                    "\nxhr.status: " + xhr.status);
            }
        }
        
        //$.ajax({
        //    async: true,
        //    type: 'POST',
        //    url: window.location.origin + '/BuildingDemolitionInfo/SaveBuildingDemolitionInfo',
        //    processData: false,
        //    contentType: false,
        //    data: { buildingDemolitionInfoVM },
        //    success: function (ind) {
        //        if (ind != 1) {
        //            alert("Ошибка сохранения информации о сносе!");
        //        }
        //    }
        //});
    }
}
let buildingDemolitionInfoCancelClick = function (event) {
    event.preventDefault();
    _buildingDemolitionInfo.updateData();
    _buildingDemolitionInfo.updateDataInElements();
}
let actFileLinkClick = function (elem, event) {
    event.preventDefault();
    let actFileBlock = elem.parents('.act-file-block');
    let idFile = actFileBlock.data('id');
    if (idFile === '')
        return;
    window.location = window.location.origin + '/BuildingDemolitionInfo/GetActFile?idFile=' + idFile;
}
let actFileUploadChange = function (elem) {
    let actFileBlock = elem.parents('.act-file-block');
    let actFile = actFileBlock.find('.act-file');
    let fileName = elem.val().replace(/\\/g, '/').replace(/.*\//, '');
    //значение 0 для всех вновь добавленых файлов
    actFileBlock.data('id', '0');
    actFileBlock.attr('data-id', '0');
    actFileBlock.find('.act-file-link').text(fileName);
    actFileBlock.find('.act-file-add').hide();
    actFile.attr('title', fileName);
    actFile.show();
}
let closeBadgeClick = function (elem) {
    let actFileBlock = elem.parents('.act-file-block');
    actFileBlock.find('.act-file-add').show();
    actFileBlock.find('.act-file').hide();
    actFileBlock.find('.act-file-upload').val('');
    let newIdFile = ''; //пустой файл
    //если раньше был прикреплен файл
    if (actFileBlock.data('id') > 0) {
        newIdFile = -actFileBlock.data('id');
    }
    actFileBlock.data('id', newIdFile);
    actFileBlock.attr('data-id', newIdFile);
}
let renameIdFile = function (actFileBlock, newIdFile) {
    actFileBlock.data('id', newIdFile);
    actFileBlock.attr('data-id', newIdFile);
    actFileBlock.find('.act-file-add').attr('for', 'actFile_' + newIdFile);
    actFileBlock.find('.act-file-upload').attr('id', 'actFile_' + newIdFile);
}

let buildingDemolitionActFilesClick = function (event) {
    let elem = $(event.target);
    let tr = elem.parents('tr');
    if (elem.hasClass('oi-x')) {
        event.preventDefault();
        tr.remove();
    }
    if (elem.hasClass('act-file-link')) {
        actFileLinkClick(elem, event);
    }
    if (elem.hasClass('badge')) {
        closeBadgeClick(elem);
    }
}
let buildingDemolitionActFilesChange = function (event) {
    let elem = $(event.target);
    if (elem.hasClass('act-file-upload')) {
        actFileUploadChange(elem);
    }
}
let _buildingDemolitionInfo;//глобальный объект
$(function () {
    let rootElem = $('#buildingDemolitionInfoForm');
    _buildingDemolitionInfo = new BuildingDemolitionInfo(rootElem);
    _buildingDemolitionInfo.updateData();
    _buildingDemolitionInfo.drawActFiles();
    console.log(_buildingDemolitionInfo);

    $('#buildingDemolitionInfoBlock').hide();
    //initializeVilidationTrs();

    $('#buildingDemolitionInfoToggle').on('click', $('#buildingDemolitionInfoBlock'), elementToogle);

    $('#buildingDemolitionActFileAdd').click(buildingDemolitionActFileAddClick);
    $('#buildingDemolitionInfoSave').click(buildingDemolitionInfoSaveClick);
    $('#buildingDemolitionInfoCancel').click(buildingDemolitionInfoCancelClick);

    $('#buildingDemolitionActFiles').click(buildingDemolitionActFilesClick);
    $('#buildingDemolitionActFiles').change(buildingDemolitionActFilesChange);
});