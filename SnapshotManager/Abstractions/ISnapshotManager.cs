using SnapshotManager.Core;
using SnapshotManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.Abstruactions
{
    /// <summary>
    /// 快照管理器接口。
    /// 定义了快照的存储、检索以及差异对比的核心功能。
    /// </summary>
    /// <typeparam name="T">被管理的数据类型。</typeparam>
    public interface ISnapshotManager<T>
    {
        /// <summary>
        /// 添加一个已构建的快照对象。
        /// </summary>
        /// <param name="snapshot">快照实例。</param>
        void AddSnapshot(Snapshot<T> snapshot);
        
        /// <summary>
        /// 添加一个快照对象，并指定存储的键名。
        /// </summary>
        /// <param name="key">用于检索快照的唯一键。</param>
        /// <param name="snapshot">快照实例。</param>
        void AddSnapshot(string key, Snapshot<T> snapshot);

        /// <summary>
        /// [快捷方法] 直接对当前数据创建快照。
        /// <para>管理器会自动生成一个基于时间戳的唯一键。</para>
        /// </summary>
        /// <param name="data">当前数据（将自动进行深拷贝）。</param>
        /// <returns>生成的快照键名。</returns>
        string TakeSnapshot(T data);

        /// <summary>
        /// [快捷方法] 直接对当前数据创建快照，并指定键名。
        /// </summary>
        /// <param name="key">快照键名。</param>
        /// <param name="data">当前数据（将自动进行深拷贝）。</param>
        void TakeSnapshot(string key, T data);

        /// <summary>
        /// 根据键名获取快照。
        /// </summary>
        /// <param name="name">快照键名。</param>
        /// <returns>快照实例。</returns>
        /// <exception cref="KeyNotFoundException">当指定键名的快照不存在时抛出。</exception>
        Snapshot<T> GetSnapshot(string name);

        /// <summary>
        /// 列出所有已存储的快照，按时间戳排序。
        /// </summary>
        /// <returns>快照列表。</returns>
        IEnumerable<Snapshot<T>> ListSnapshots();

        /// <summary>
        /// 对比两个历史快照之间的差异。
        /// </summary>
        /// <param name="snapA">基准快照的键名。</param>
        /// <param name="snapB">目标快照的键名。</param>
        /// <returns>差异节点树。</returns>
        DiffNode Diff(string snapA, string snapB);

        /// <summary>
        /// [实时对比] 对比历史快照与当前内存数据的差异。
        /// <para>无需将当前数据保存为快照即可进行对比。</para>
        /// </summary>
        /// <param name="baseSnapKey">作为基准的历史快照键名。</param>
        /// <param name="currentData">当前的内存数据。</param>
        /// <returns>差异节点树。</returns>
        DiffNode DiffWith(string baseSnapKey, T currentData);
    }

}
