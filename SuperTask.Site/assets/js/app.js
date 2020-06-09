$(function () {
	//$('form .multiselect').multiselect();
})

// ajax

$.ajaxSetup({ cache: false });
var ajaxErrorMessage = "操作出现异常！";

$(function () {

	// ajax modal
	$(document).on('click.bs.modal.data-api', '[data-toggle="ajax-modal"]', function (event) {

		var $this = $(this),
			url = $this.data('url'),
			$target = $($this.data('target'));

		$.get(url, function (response) {
			// ajax get form content
			$target
				.html(response)
				.modal({ backdrop: 'static', keyboard: false })
				.find('.form-control').first().focus();
		});

	});


});

// boot-grid

var gridOptions = {

	ajax: true,

	padding: 4,

	rowCount: [10, 20, 50, 100],

	labels: {
		//all: "全部",
		infos: "显示 {{ctx.start}} 到 {{ctx.end}} 共 {{ctx.total}} 记录",
		loading: "数据加载中...",
		noResults: "无可寻记录",
		refresh: "重新加载",
		search: "搜索"
	},

	//css: {
	//	icon: "icon_n glyphicon",
	//},

	templates: {
		footer: "<div id=\"{{ctx.id}}\" class=\"{{css.footer}}\"><div class=\"row infoBar\"><p class=\"{{css.pagination}}\"></p><p class=\"{{css.infos}}\"></p></div></div>"
	},

	formatters: {

		"Boolean": function (column, row) { return row[column.id] ? "<i class=\"fa fa-check-square-o fa-lg\"></i>" : ""; },

		"Current": function (column, row) { return "$" + row[column.id]; },

		"DateTime": function (column, row) { return row[column.id] ? moment(row[column.id]).format('YYYY-MM-DD hh:mm:ss') : ''; },

		"DateOnly": function (column, row) { return row[column.id] ? moment(row[column.id]).format('YYYY-MM-DD') : ''; },

		"Email": function (column, row) { return "<a href='mailto:" + row[column.id] + "'>" + row[column.id] + "</a>"; },

		"Percent": function (column, row) { return row[column.id] + " %"; },

		"TimeOnly": function (column, row) { return row[column.id]; },

		"Url": function (column, row) {
			var text = row[column.id], url = text;
			if (!(typeof text === "string")) { url = text.url; text = text.text; }
			return "<a href='" + url + "'>" + text + "</a>";
		},

	},

	responseHandler: function (response) {
		if (response.errMessage) {
			alert(response.errMessage);
			return { current: 1, rowCount: 10, rows: [], total: 0 };
		}
		else {
			return response;
		}
	},

	loadDataErrorHandler: function (errMessage) {
		alert(errMessage);
	}

};


// summer-edit upload file

function sendFile(file, node) {
	data = new FormData();
	data.append("file", file);

	var fileData = URL.createObjectURL(file);
	node.summernote('insertImage', fileData, function ($image) {
		$.ajax({
			url: "/Attachment/Image",
			data: data,
			cache: false,
			contentType: false,
			processData: false,
			dataType: "json",
			type: 'POST',
			success: function (result) {
				$image.attr('src', result.showUrl);
			}
		});

	});
}

// date range picker

var dateRangeOption = {
	"autoApply": true,
	"locale": {
		"format": "YYYY-MM-DD",
		"separator": " 至 ",
		"applyLabel": "Apply",
		"cancelLabel": "Cancel",
		"fromLabel": "From",
		"toLabel": "To",
		"customRangeLabel": "Custom",
		"daysOfWeek": ["日", "一", "二", "三", "四", "五", "六"],
		"monthNames": ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
		"firstDay": 1
	}
}

// toastmessage
function popupMessage(data, then) {
	$().toastmessage('showToast', { text: data.msg, sticky: data.result != 'success', type: data.result });
	if (then && jQuery.isFunction(then[data.result]))
		then[data.result](data);
}

// viewModel

function findInViewModel(key, value, fn, list) {
	list = list || viewModel.dataList;
	$.each(list(), function (k, d) {
		if (d[key] == value) {
			fn(d, k, list); // data, index, list
			return false;
		}
	});
}

function ajaxBindFileUpload() {
	// dropzone
	Dropzone.autoDiscover = false;
	$('.dropzone').dropzone({
		addedContainer: '#flyArea',
		dictResponseError: '上传出错',
		uploadMultiple: false,
		maxFilesize: 200,
		init: function () {
			this.on('success', function (file, data) {
				alert();
				$('#AttachmentUrl').val(data.url);
				$('#AttachmentName').val(data.filename);
				setTimeout(function () { $('#uploadName').html(data.filename); }, 1000);
			});
			this.on('error', function (file, message) {
				popupMessage({ result: 'error', msg: message });
			});
		}
	});

	// proxy click event to dropzone.
	$('#btn-upload').on('click', function () {
		$('.dropzone').trigger('click');
	});
}

function ajaxSubmitForm(selector, isReplaceCommas) {
	$.validator.unobtrusive.parse(selector);

	selector.submit(function (e) {
		e.preventDefault();
		var $this = $(this);
		var para = $this.serialize();
		if (isReplaceCommas == true) {
			para = para.replace('%2c', ',');
		}
		$this.valid() && $.post($this.attr('action'), para, function (data, status) {
			popupMessage(data, {
				success: function () {
					var afterSuccess = $this.data('afterSuccess');
					if (afterSuccess) {
						eval(afterSuccess);
					}
				}
			});
		})
	});
}


Date.prototype.Format = function (fmt) { //author: meizz
	var o = {
		"M+": this.getMonth() + 1, //月份
		"d+": this.getDate(), //日
		"h+": this.getHours(), //小时
		"m+": this.getMinutes(), //分
		"s+": this.getSeconds(), //秒
		"q+": Math.floor((this.getMonth() + 3) / 3), //季度
		"S": this.getMilliseconds() //毫秒
	};
	if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
	for (var k in o)
		if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
	return fmt;
}


function getValuesByCheckboxList(checkbox, callback) {
	$(checkbox).on('click', function () {

		$(this).attr('checked', !$(this).attr('checked'));
		tempStr = '';

		$(checkbox).each(function () {
			var $this = $(this);
			if ($this.attr('checked') == 'checked')
				tempStr += ',' + $(this).val();
		});

		if (callback) {
			callback(tempStr);
		}
	});
}


function bindDropDownByAjax(dp, url, para, cb, dv) {
	var $dp = $(dp);

	$.post(url, para, function (o) {
		$dp.empty();

		$(o.data).each(function (i) {
			if (this.Value === dv) 
				$dp.append('<option selected="selected" value="' + this.Value + '">' + this.Text + '</option>');
			else
				$dp.append('<option value="' + this.Value + '">' + this.Text + '</option>');
		});

		$dp.parent().find('.searchable-select').remove();
		$dp.searchableSelect({
			afterSelectItem: function (v) {
				if (cb)
					cb(v, o);
			}
		});

	});
}


function initTableCheckbox(tableId) {
	var table = '#' + tableId;
	var $thr = $(table + ' thead tr');
	var $checkAllTh = $('<th class="width30"><input type="checkbox" id="checkAll" name="checkAll" /></th>');
	if ($('#checkAll').size() <= 0) {
		/*将全选/反选复选框添加到表头最前，即增加一列*/
		$thr.prepend($checkAllTh);
	}
	/*“全选/反选”复选框*/
	var $checkAll = $thr.find('input');
	$checkAll.click(function (event) {
		/*将所有行的选中状态设成全选框的选中状态*/
		$tbr.find('input').prop('checked', $(this).prop('checked'));
		/*并调整所有选中行的CSS样式*/
		if ($(this).prop('checked')) {
			$tbr.find('input').parent().parent().addClass('warning');
		} else {
			$tbr.find('input').parent().parent().removeClass('warning');
		}
		/*阻止向上冒泡，以防再次触发点击操作*/
		event.stopPropagation();
	});
	/*点击全选框所在单元格时也触发全选框的点击操作*/
	$checkAllTh.click(function () {
		$(this).find('input').click();
	});
	var $tbr = $(table + ' tbody tr');
	var $checkItemTd = $('<td><input type="checkbox" name="checkItem" class="checkItem"/></td>');
	/*每一行都在最前面插入一个选中复选框的单元格*/
	$tbr.prepend($checkItemTd);
	/*点击每一行的选中复选框时*/
	$tbr.find('input').click(function (event) {
		/*调整选中行的CSS样式*/
		$(this).parent().parent().toggleClass('warning');
		/*如果已经被选中行的行数等于表格的数据行数，将全选框设为选中状态，否则设为未选中状态*/
		$checkAll.prop('checked', $tbr.find('input:checked').length == $tbr.length ? true : false);
		/*阻止向上冒泡，以防再次触发点击操作*/
		event.stopPropagation();
	});
	/*点击每一行时也触发该行的选中操作*/
	$tbr.click(function () {
		$(this).find('input').click();
	});
}

//function ajaxBindFileUpload() {
//	// dropzone
//	Dropzone.autoDiscover = false;
//	$('.dropzone').dropzone({
//		addedContainer: '#flyArea',
//		dictResponseError: '上传出错',
//		dictFileTooBig: '上传文件大小({{filesize}}MiB) 最大文件大小 ({{maxFilesize}}MiB)',
//		uploadMultiple: false,
//		maxFilesize: 200,
//		init: function () {
//			this.on("processing", function (i) {
//				$('.progress').remove();
//				$('#uploadName').parent().parent().append(function () {
//					return '<div class="progress"><div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%;"><span class="sr-only"></span></div></div>';
//				})
//			})
//			this.on("totaluploadprogress", function (file, progress, bytesSent) {
//				$(".progress-bar").css("width", parseInt(file) + "%");
//			})
//			this.on('success', function (file, data) {
//				var url = $('#AttachmentUrl').val();
//				url = url.length > 0 ? url + "|" : url;
//				var name = $('#AttachmentName').val();
//				name = name.length > 0 ? name + "|" : name;
//				$('#AttachmentUrl').val(url + data.url);
//				$('#AttachmentName').val(name + data.filename);
//				var showName = data.filename.length > 40 ? data.filename.substring(0, 37) + "..." : data.filename;
//				$('#uploadName').append('<p><label title="' + data.filename + '">' + showName + '</label> <button type="button" class="btn btn-danger btn-xs btn-delete" onclick="delAttachment(this)">删除</button></p>');
//				//setTimeout(function () {  }, 1000);
//			});
//			this.on('error', function (file, message) {
//				popupMessage({ result: 'error', msg: message });
//			});
//		}
//	});

//	// proxy click event to dropzone.
//	$('#btn-upload').on('click', function () {
//		$('.dropzone').trigger('click');
//		$(".progress").hide();
//		$(".progress-bar").attr('style', 'width:0%');
//	});
//}

////	删除附件

//function delAttachment(e) {

//	var current = $('.btn-delete').index(e);

//	var name = $('#AttachmentName').val();
//	var url = $('#AttachmentUrl').val();

//	var nameArray = name.split('|');
//	var tempName = '';
//	$.each(nameArray, function (index, item) {
//		if (current != index) {
//			tempName += item + '|';
//		}
//	})

//	var urlArray = url.split('|');
//	var tempUrl = '';
//	$.each(urlArray, function (index, item) {
//		if (current != index) {
//			tempUrl += item + '|';
//		}
//	})

//	tempName = tempName.length > 0 ? tempName.substring(0, tempName.lastIndexOf('|')) : tempName;
//	tempUrl = tempUrl.length > 0 ? tempUrl.substring(0, tempUrl.lastIndexOf('|')) : tempUrl;

//	$('#AttachmentName').val(tempName);
//	$('#AttachmentUrl').val(tempUrl);
//	$('#uploadName p:eq(' + current + ')').remove();
//	$(".progress").remove();
//}