﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bing.Events.Handlers;
using Bing.Events.Messages;
using Bing.Logs;
using Bing.Logs.Aspects;
using Bing.Logs.Extensions;

namespace Bing.Events.Default
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public class EventBus : IEventBus
    {
        /// <summary>
        /// 事件处理器工厂
        /// </summary>
        public IEventHandlerFactory Factory { get; set; }

        /// <summary>
        /// 消息事件总线
        /// </summary>
        public IMessageEventBus MessageEventBus { get; set; }

        /// <summary>
        /// 初始化一个<see cref="EventBus"/>类型的实例
        /// </summary>
        /// <param name="factory">事件处理器工厂</param>
        /// <param name="messageEventBus">消息事件总线</param>
        public EventBus(IEventHandlerFactory factory, IMessageEventBus messageEventBus = null)
        {
            Factory = factory;
            MessageEventBus = messageEventBus;
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>        
        [TraceLog]        
        public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            SyncHandle(@event);
            if (@event is IMessageEvent messageEvent)
            {
                AsyncHandle(messageEvent);
            }
        }

        /// <summary>
        /// 同步处理 - 在当前线程处理
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        private void SyncHandle<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var handlers = Factory.GetHandlers<TEvent>();

            handlers?.Select(eventHandler => Task.Factory.StartNew(() =>
                {
                    HandleEvent(eventHandler, @event);                    
                })
            ).ToArray();            
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        /// <param name="event">事件</param>        
        private void HandleEvent<TEvent>(IEventHandler<TEvent> handler, TEvent @event) where TEvent : IEvent
        {
            handler.Handle(@event);
        }

        /// <summary>
        /// 异步处理 - 发送到消息中间件
        /// </summary>        
        /// <param name="messageEvent">消息事件</param>
        private void AsyncHandle(IMessageEvent messageEvent)
        {
            MessageEventBus?.Publish(messageEvent);
        }
    }
}
