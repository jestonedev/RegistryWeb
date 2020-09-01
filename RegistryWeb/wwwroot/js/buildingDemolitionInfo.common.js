'use strict';

let BuildingDemolitionActFile = class {
    constructor(idBuilding) {
        this.id = 0;
        this.idBuilding = idBuilding;
        this.idActFile = "";
        this.idActTypeDocument = 0;
        this.number = "";
        this.date = "";
        this.name = "";
        this.originalNameActFile = "";
        this.fileInput = "";
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

    get fileInput() {
        return this._fileInput;
    }
    set fileInput(value) {
        this._fileInput = value;
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

        this.fileInput = tr.find('.act-file-upload')[0];
        if (this.fileInput.files[0] !== undefined) {
            this.file = this.fileInput.files[0];
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
        this.number = jsonBuildingDemolitionActFiles.number === null ? "" : jsonBuildingDemolitionActFiles.number;
        this.date = jsonBuildingDemolitionActFiles.date;
        this.name = jsonBuildingDemolitionActFiles.name === null ? "" : jsonBuildingDemolitionActFiles.name;
        this.originalNameActFile = jsonBuildingDemolitionActFiles.originalNameActFile;
        this.file = "";
        this.fileInput = "";
    }
};

let BuildingDemolitionInfo = class {
    constructor(form) {
        this.form = form;
        this._action = form.data('action');
        this._canEditExtInfo = JSON.parse(form.data("caneditextinfo").toLowerCase());
        this.idBuilding = +this.form.data('idbuilding');
    }

    get form() {
        return this._form;
    }
    set form(value) {
        this._form = value;
    }

    get action() {
        return this._action;
    }
    set action(value) {
        this._action = value;
    }

    get canEditExtInfo() {
        return this._canEditExtInfo;
    }
    set canEditExtInfo(value) {
        this._canEditExtInfo = value;
    }

    get idBuilding() {
        return this._idBuilding;
    }
    set idBuilding(value) {
        this._idBuilding = value;
    }

    get demolishedPlanDate() {
        return this._demolishPlanDate;
    }
    set demolishedPlanDate(value) {
        this._demolishPlanDate = value;
    }

    get demolishedFactDate() {
        return this._demolishFactDate;
    }
    set demolishedFactDate(value) {
        this._demolishFactDate = value;
    }

    get dateOwnerEmergency() {
        return this._dateOwnerEmergency;
    }
    set dateOwnerEmergency(value) {
        this._dateOwnerEmergency= value;
    }

    get demandForDemolishingDeliveryDate() {
        return this._demandForDemolishingDeliveryDate;
    }
    set demandForDemolishingDeliveryDate(value) {
        this._demandForDemolishingDeliveryDate = value;

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

    getJson = function () {
        this.initialize();
        let buildingDemolitionInfoJson = {
            IdBuilding: this.idBuilding,
            DemolishedPlanDate: this.demolishedPlanDate,
            DemolishedFactDate: this.demolishedFactDate,
            DateOwnerEmergency: this.dateOwnerEmergency,
            DemandForDemolishingDeliveryDate: this.demandForDemolishingDeliveryDate,
            BuildingDemolitionActFiles: []
        };
        for (let i = 0; i < this.buildingDemolitionActFiles.length; i++) {
            buildingDemolitionInfoJson.BuildingDemolitionActFiles[i] = {
                Id: this.buildingDemolitionActFiles[i].id,
                IdBuilding: this.buildingDemolitionActFiles[i].idBuilding,
                IdActFile: this.buildingDemolitionActFiles[i].idActFile,
                IdActTypeDocument: this.buildingDemolitionActFiles[i].idActTypeDocument,
                Number: this.buildingDemolitionActFiles[i].number,
                Date: this.buildingDemolitionActFiles[i].date,
                Name: this.buildingDemolitionActFiles[i].name,
                File: this.buildingDemolitionActFiles[i].file,
                FileInput: this.buildingDemolitionActFiles[i].fileInput
            };
        }
        return buildingDemolitionInfoJson;
    }

    initialize = function () {
        this.form = $('#buildingDemolitionInfoForm');
        this._action = this.form.data('action');
        this._canEditExtInfo = JSON.parse(this.form.data('caneditextinfo').toLowerCase());
        this.idBuilding = +this.form.data('idbuilding');
        this.demolishedPlanDate = this.form.find('#demolishPlanDate').val();
        this.demolishedFactDate = this.form.find('#demolishFactDate').val();
        this.dateOwnerEmergency = this.form.find('#dateOwnerEmergency').val();
        this.demandForDemolishingDeliveryDate = this.form.find('#demandForDemolishingDeliveryDate').val();
        this.buildingDemolitionActFiles = [];

        let trs = this.form.find('#buildingDemolitionActFiles>tr');
        for (let i = 0; i < trs.length; i++) {
            this.buildingDemolitionActFiles[i] = new BuildingDemolitionActFile(this.idBuilding);
            this.buildingDemolitionActFiles[i].initialize($(trs[i]));
        }
    }

    setAllProperty = function (jsonBuildingDemolitionActFiles) {
        this.idBuilding = jsonBuildingDemolitionActFiles.idBuilding;
        this.demolishedPlanDate = jsonBuildingDemolitionActFiles.demolishedPlanDate;
        this.demolishedFactDate = jsonBuildingDemolitionActFiles.demolishedFactDate;
        this.dateOwnerEmergency = jsonBuildingDemolitionActFiles.dateOwnerEmergency;
        this.demandForDemolishingDeliveryDate = jsonBuildingDemolitionActFiles.demandForDemolishingDeliveryDate;
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
        buildingDemolitionInfoVM.append('DemolishedPlanDate', this.demolishedPlanDate);
        buildingDemolitionInfoVM.append('DemolishedFactDate', this.demolishedFactDate);
        buildingDemolitionInfoVM.append('DateOwnerEmergency', this.dateOwnerEmergency);
        buildingDemolitionInfoVM.append('DemandForDemolishingDeliveryDate', this.demandForDemolishingDeliveryDate);
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
        }
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
        this.form.find('#buildingDemolitionActFiles').append(this.createTr(obj));
        this.drawActFiles(this.form.find('#buildingDemolitionActFiles>tr').last());
    }

    createTr = function (buildingDemolitionActFile) {
        let originalName = "";
        let idFile = ""; //значение null для пустого файла
        let id = "actFile_" + uuidv4();
        if (buildingDemolitionActFile.idActFile !== "" && buildingDemolitionActFile.idActFile !== null) {
            idFile = buildingDemolitionActFile.idActFile;
            id = "actFile_" + idFile;
            originalName = buildingDemolitionActFile.originalNameActFile;
        }
        buildingDemolitionActFile.idActFile == null ? "" : buildingDemolitionActFile.idActFile;
        let tr = `
        <tr class="building-demolition-act-file" data-idbuildingdemolitionactfile="${buildingDemolitionActFile.id}">
            <td class="align-middle">
                <input type="hidden" value="${buildingDemolitionActFile.id}" name="Id" />
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
                    <label for="${id}" class="btn btn-success act-file-add">
                        <span class="oi oi-paperclip"></span>
                    </label>
                    <div class="act-file" title="${originalName}">
                        <a href="#" class="btn-link act-file-link">${originalName}</a>
                        <a class="badge btn btn-danger" href="#">&times;</a>
                    </div>
                </div>
            </td>
            <td class="align-middle">
                <a href="#" class="btn btn-danger act-file-record-remove" title="Удалить" aria-label="Удалить">
                    <span class="oi oi-x"></span>
                </a>
            </td>
        </tr>
        `;
        return tr;
    }

    updateDataInElements = function () {
        this.form.data('idbuilding', this.idBuilding);
        this.form.attr('data-idbuilding', this.idBuilding);

        this.form.find('#demolishPlanDate').val(this.demolishedPlanDate);
        this.form.find('#demolishFactDate').val(this.demolishedFactDate);
        this.form.find('#dateOwnerEmergency').val(this.dateOwnerEmergency);
        this.form.find('#demandForDemolishingDeliveryDate').val(this.demandForDemolishingDeliveryDate);

        this.form.find('#buildingDemolitionActFiles>tr').remove();
        if (this.buildingDemolitionActFiles.length == 0) return;
        for (let i = 0; i < this.buildingDemolitionActFiles.length; i++) {
            this.form.find('#buildingDemolitionActFiles').append(this.createTr(this.buildingDemolitionActFiles[i]));
        }
        this.drawActFiles();
    }

    drawActFiles = function (trs = this.form.find('#buildingDemolitionActFiles>tr')) {
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
};

let buildingDemolitionActFileAddClick = function (event) {
    event.preventDefault();
    _buildingDemolitionInfo.addNewTr();
};
let editFieldsAndBtnsDisabled = function (isDisabled) {
    $('#buildingDemolitionInfoForm input').prop('disabled', isDisabled);
    $('#buildingDemolitionInfoForm select').prop('disabled', isDisabled);
    if (isDisabled) {
        $('#buildingDemolitionInfoForm .act-file-record-remove').addClass('disabled');
        $('#buildingDemolitionActFileAdd').addClass('disabled');
        $('#buildingDemolitionInfoForm .badge').addClass('disabled');
        $('#buildingDemolitionInfoForm .act-file-add').addClass('disabled');
    } else {
        $('#buildingDemolitionInfoForm .act-file-record-remove').removeClass('disabled');
        $('#buildingDemolitionActFileAdd').removeClass('disabled');
        $('#buildingDemolitionInfoForm .badge').removeClass('disabled');
        $('#buildingDemolitionInfoForm .act-file-add').removeClass('disabled');
    }
};
let buildingDemolitionInfoEditClick = function (event) {
    event.preventDefault();
    $('#buildingDemolitionInfoSave').show();
    $('#buildingDemolitionInfoCancel').show();
    $('#buildingDemolitionInfoEdit').hide();
    editFieldsAndBtnsDisabled(false);
};
let buildingDemolitionInfoSaveClick = function (event) {
    event.preventDefault();
    if ($('#buildingDemolitionInfoForm').valid()) {
        let formData = _buildingDemolitionInfo.getFormData();
        let xhr = new XMLHttpRequest();
        xhr.open("POST", window.location.origin + '/BuildingDemolitionInfo/SaveBuildingDemolitionInfo');
        xhr.send(formData);
        xhr.onreadystatechange = function () {
            if (this.readyState !== 4) return;
            var response = JSON.parse(xhr.responseText);
            if (xhr.status !== 200 || !(response instanceof Array)) {
                alert("Ошибка сохранения информации о сносе!" +
                    "\nxhrresponseText: " + xhr.responseText +
                    "\nxhr.status: " + xhr.status);
                return;
            }
            $('#buildingDemolitionInfoSave').hide();
            $('#buildingDemolitionInfoCancel').hide();
            $("#buildingDemolitionInfoBlock").removeClass("list-group-item-warning");
            $('#buildingDemolitionInfoEdit').show();
            editFieldsAndBtnsDisabled(true);
            $(".act-file-block").each(function (idx, elem) {
                $(this).data("id", response[idx] === null || response[idx] === undefined ? "" : response[idx]);
                $(this).attr("data-id", response[idx] === null || response[idx] === undefined ? "" : response[idx]);
            });
            $("#buildingDemolitionActFiles input[title], #buildingDemolitionActFiles select[title]").each(function (idx, elem) {
                if (elem.tagName === "SELECT") {
                    $(elem).attr("title", $(elem).find("option[value='" + $(elem).val()+"']").text());
                } else {
                    $(elem).attr("title", $(elem).val());
                }
            });
        };
    }
};
let buildingDemolitionInfoCancelClick = function (event) {
    event.preventDefault();
    _buildingDemolitionInfo.updateData();
    _buildingDemolitionInfo.updateDataInElements();
    $('#buildingDemolitionInfoSave').hide();
    $('#buildingDemolitionInfoCancel').hide();
    $("#buildingDemolitionInfoBlock").removeClass("list-group-item-warning");
    $('#buildingDemolitionInfoEdit').show();
    editFieldsAndBtnsDisabled(true);
};
let actFileLinkClick = function (elem, event) {
    event.preventDefault();
    let actFileBlock = elem.parents('.act-file-block');
    let idFile = actFileBlock.data('id');
    if (idFile === '')
        return;
    window.location = window.location.origin + '/BuildingDemolitionInfo/GetActFile?idFile=' + idFile;
};
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
};
let closeBadgeClick = function (elem, event) {
    event.preventDefault();
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
};
let renameIdFile = function (actFileBlock, newIdFile) {
    actFileBlock.data('id', newIdFile);
    actFileBlock.attr('data-id', newIdFile);
    actFileBlock.find('.act-file-add').attr('for', 'actFile_' + newIdFile);
    actFileBlock.find('.act-file-upload').attr('id', 'actFile_' + newIdFile);
};

let buildingDemolitionActFilesClick = function (event) {
    let elem = $(event.target);
    let tr = elem.parents('tr');
    if (elem.hasClass('act-file-record-remove') || elem.hasClass('oi-x')) {
        event.preventDefault();
        tr.remove();
    }
    if (elem.hasClass('act-file-link')) {
        actFileLinkClick(elem, event);
    }
    if (elem.hasClass('badge')) {
        closeBadgeClick(elem, event);
    }
};
let buildingDemolitionActFilesChange = function (event) {
    let elem = $(event.target);
    if (elem.hasClass('act-file-upload')) {
        actFileUploadChange(elem);
    }
};
let _buildingDemolitionInfo;//глобальный объект
$(function () {
    let rootElem = $('#buildingDemolitionInfoForm');
    _buildingDemolitionInfo = new BuildingDemolitionInfo(rootElem);
    _buildingDemolitionInfo.updateData();
    _buildingDemolitionInfo.drawActFiles();
    
    $('#buildingDemolitionInfoSave').hide();
    $('#buildingDemolitionInfoCancel').hide();
    if (_buildingDemolitionInfo.action != "Create") {
        editFieldsAndBtnsDisabled(true);
    }
    if (_buildingDemolitionInfo.action == "Create" && !_buildingDemolitionInfo.canEditExtInfo) {
        _buildingDemolitionInfo.form.hide();
    }
    //initializeVilidationTrs();

    $('#buildingDemolitionInfoToggle').on('click', $('#buildingDemolitionInfoBlock'), elementToogleHide);

    $('#buildingDemolitionActFileAdd').click(buildingDemolitionActFileAddClick);
    $('#buildingDemolitionInfoEdit').click(buildingDemolitionInfoEditClick);
    $('#buildingDemolitionInfoSave').click(buildingDemolitionInfoSaveClick);
    $('#buildingDemolitionInfoCancel').click(buildingDemolitionInfoCancelClick);

    $('#buildingDemolitionActFiles').click(buildingDemolitionActFilesClick);
    $('#buildingDemolitionActFiles').change(buildingDemolitionActFilesChange);
});