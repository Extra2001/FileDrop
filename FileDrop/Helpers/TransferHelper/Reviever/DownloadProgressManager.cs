﻿using FileDrop.Helpers.Dialog;
using FluentFTP;
using System;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public class DownloadProgressManager : IProgress<FtpProgress>
    {
        public void Report(FtpProgress value)
        {
            ModelDialog.ShowWaiting("正在接收文件", $"进度：{value.Progress}%，速度：{SpeedParser.Parse(value.TransferSpeed)}，剩余时间：{(int)value.ETA.TotalMinutes}分{value.ETA.Seconds}秒");
        }
    }
}
