using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/**
**该题目校招岗位可以不作答，社招需要作答。**

按照要求在 {@link Q3.onStartBtnClick} 中编写一段异步任务处理逻辑，具体执行步骤如下：
1. 调用 {@link Q3.loadConfig} 加载配置文件，获取资源列表
2. 根据资源列表调用 {@link Q3.loadFile} 加载资源文件
3. 资源列表中的所有文件加载完毕后，调用 {@link Q3.initSystem} 进行系统初始化
4. 系统初始化完成后，打印日志

附加要求
1. 加载文件时，需要做并发控制，最多并发 3 个文件
2. 加载文件时，需要添加超时控制，超时时间为 3 秒
3. 加载文件失败时，需要对单文件做 backoff retry 处理，重试次数为 3 次
4. 对错误进行捕获并打印输出
*/

public class Q3 : MonoBehaviour
{
    public class LoadFileTask
    {
        public string FileName;
        public int RetryCount;
        public string LoadResult;
    }
    
    private const int MaxLoadingTaskCount = 3;
    private const int TimeoutMilliseconds = 3000;
    private const int MaxRetryCount = 3;
    public async void OnStartBtnClick()
    {
        // TODO: 请在此处开始作答
       
        try
        { 
            string[] result =  await LoadConfig();
            List<Task> tasks = new List<Task>();
            SemaphoreSlim semaphore = new SemaphoreSlim(MaxLoadingTaskCount);

            Queue<LoadFileTask> failedTasks = new();
            
            for (int i = 0; i < result.Length; i++)
            {
                await semaphore.WaitAsync();
                LoadFileTask fileTask = new LoadFileTask()
                {
                    FileName = result[i],
                    RetryCount = 0,
                    LoadResult = string.Empty,
                };
                
                Task t = StartTask(fileTask, TimeoutMilliseconds).ContinueWith((task) =>
                {
                    semaphore.Release();
                    if (!string.IsNullOrEmpty( task.Result.LoadResult))
                    {
                        failedTasks.Enqueue(task.Result);
                    }
                });
                
                tasks.Add(t);
                
            }

            await Task.WhenAll(tasks);
            
            await InitSystem();
            while (failedTasks.Count > 0)
            {
                Debug.Log(failedTasks.Dequeue().LoadResult);
            }

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task<LoadFileTask> StartTask(LoadFileTask task,int timeoutMilliseconds)
    {
        while (task.RetryCount < MaxRetryCount)
        {
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeoutMilliseconds, cts.Token);
            var loadFileTask = LoadFile(task.FileName);
           
            try
            {
                var resultTask = await Task.WhenAny(loadFileTask, timeoutTask);
                if (resultTask == timeoutTask)
                {
                    task.LoadResult = $"Load file failed: {task.FileName}.Load file timeout";
                    task.RetryCount++;
                }
                else
                {
                    task.RetryCount = 0;
                    task.LoadResult= string.Empty;
                    break;
                }
            }
            catch (Exception e)
            {
                task.LoadResult = e.Message;
                task.RetryCount++;
            }
            finally
            {
                cts.Dispose();
            }
        }
        return task;
    }

    // #region 以下是辅助测试题而写的一些 mock 函数，请勿修改

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>文件列表</returns>
    public async Task<string[]> LoadConfig()
    {
        Debug.Log("load config start");
        await Task.Delay(1000);
        if (Random.value > 0.01f)
        {
            Debug.Log("load config success");
            string[] files = new string[100];
            for (int i = 0; i < 100; i++)
            {
                files[i] = $"file-{i}";
            }
            return files;
        }
        else
        {
            Debug.Log("load config failed");
            throw new System.Exception("Load config failed");
        }
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public async Task LoadFile(string file)
    {
        Debug.Log($"load file start: {file}");
        await Task.Delay(Random.Range(1000, 5000));
        if (Random.value > 0.01f)
        {
            Debug.Log($"load file success: {file}");
        }
        else
        {
            Debug.Log($"load file failed: {file}");
            throw new System.Exception($"Load file failed: {file}");
        }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    public async Task InitSystem()
    {
        Debug.Log("init system start");
        await Task.Delay(1000);
        Debug.Log("init system success");
    }

    // #endregion
}
