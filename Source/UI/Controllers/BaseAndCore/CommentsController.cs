using AutoMapper;
using Newtonsoft.Json;
using Sasha.Lims.Core.DTO.Common;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.WebUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Sasha.Lims.WebUI.Infrastructure.Helpers;

namespace Sasha.Lims.WebUI.Controllers
{
	[Authorize]
	public class CommentsController : BaseController
	{
		private readonly ICommentsService _commentService;
		public CommentsController(ICommentsService service) 
		{
			_commentService = service;
		}

		public ActionResult CommentForm(int? rowId, int? tableId)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			if (tableId == null) tableId = (int) TableType.Sample;
			ViewBag.rowId = rowId;
			ViewBag.tableId = tableId;
			return View("CommentForm");
		}
		public JsonResult SaveComment(int? rowId, int? id, int? tableId) 
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			var changedComment = new CommentViewModel();
			JsonConvert.PopulateObject(bodyText, changedComment);
			var comments = _commentService.GetComments(rowId, (TableType) tableId);//get comment

			var changedCommentDTO = comments.FirstOrDefault(x => x.Id == id);//find
			changedCommentDTO.Comment = changedComment.Comment;//Set chanded text
			var user = base.GetCurrentUser();
			changedCommentDTO.CreatedByUserId = Convert.ToInt32(user.UserId);
			changedCommentDTO.CommentTypeId = changedComment.CommentTypeId;
			changedCommentDTO.CreatedDateTime = DateTime.Now;
			_commentService.Edit(changedCommentDTO);//save
			var commModel = ConvertDtoToModel(comments);

			return Json(commModel, JsonRequestBehavior.AllowGet);
		}

		public JsonResult DeleteComment(int? rowId, int? id, int? tableId)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			_commentService.Delete(new List<int> { (int) id });
			var modelList = GetModel(rowId, tableId);

			ViewBag.rowId = rowId;
			return Json(modelList, JsonRequestBehavior.AllowGet);
		}

		public JsonResult AddComment(int? rowId, int? tableId)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			var bodyStream = new StreamReader(HttpContext.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();

			var newComment = new CommentViewModel();
			JsonConvert.PopulateObject(bodyText, newComment);
			var user = base.GetCurrentUser();
			var comment = new CommentDTO()
			{
				Comment = newComment.Comment,
				CreatedByUserId = Convert.ToInt32(user.UserId),
				CommentTypeId = newComment.CommentTypeId,
				RowId = rowId,
				TableId = tableId
			};
			_commentService.AddComment(comment);
			var modelList = GetModel(rowId, tableId);
			ViewBag.rowId = rowId;
			return Json(modelList, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetComments(int? rowId, int? tableId)
		{
			var modelList = GetModel(rowId, tableId);
			ViewBag.rowId = rowId;
			return Json(modelList, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetCommentTypes()
		{
			var commentTypes = PropertiesController.PropertiesReader.GetProperties((int) PropsType.CommentType);
			return Json(commentTypes, JsonRequestBehavior.AllowGet);
		}

		public List<CommentViewModel> GetModel(int? rowId, int? tableId = null)
		{
			ExceptionHelper.ThrowExceptionIfRowIdIsNull(rowId);
			if (tableId == null || tableId == 0)
			{
				tableId = (int) TableType.Sample;
			}
			var comments = _commentService.GetComments(rowId, (TableType) tableId);
			return ConvertDtoToModel(comments);
		}



		private List<CommentViewModel> ConvertDtoToModel(IEnumerable<CommentDTO> comments)
		{
			List<CommentViewModel> modelList = new List<CommentViewModel>();
			foreach (var commentDTO in comments) //Set Additional Property
			{
				var commentModel = Mapper.Map<CommentDTO, CommentViewModel>(commentDTO);
				commentModel.CreatedByUser =
					GetSystemUsers().Where(x => x.UserId == commentModel.CreatedByUserId).FirstOrDefault();

				modelList.Add(commentModel);
			}
			return modelList;
		}

		public static int GetCommentCount(int rowId, TableType tableType)
		{
			ICommentsService _service = DependencyResolver.Current.GetService<ICommentsService>();
		    return _service.GetCommentsCount(rowId, tableType);
		}
	}
}