using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DataStructLib
{

    /// <summary>
    /// 实现跳表
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable
    {
        /// <summary>
        /// 跳表键值对
        /// </summary>
        /// <typeparam name="W"></typeparam>
        /// <typeparam name="X"></typeparam>
        private struct SkipListKVPair<W, X>
        {
            public W Key;
            public X Value;
            public SkipListKVPair(W key, X value)
            {
                Key = key;
                Value = value;
            }
        }
        /// <summary>
        /// 跳表节点
        /// </summary>
        /// <typeparam name="TNKey"></typeparam>
        /// <typeparam name="TNValue"></typeparam>
        private class SkipListNode<TNKey, TNValue>
        {
            public int level;
            public bool isFront = false;        //是否为链表的头节点
            public SkipListKVPair<TNKey, TNValue> keyValue;
            public SkipListNode<TNKey, TNValue> Left, Right, Down, Up;

            public TNKey Key
            {
                get
                {
                    return keyValue.Key;
                }
                set
                {
                    keyValue.Key = value;
                }
            }
            public TNValue Value
            {
                get
                {
                    return keyValue.Value;
                }
                set
                {
                    keyValue.Value = value;
                }
            }
            public SkipListNode()
            {
                keyValue = new SkipListKVPair<TNKey, TNValue>(default, default);
                isFront = true;
            }

            public SkipListNode(SkipListKVPair<TNKey, TNValue> keyPair)
            {
                keyValue = keyPair;
            }
            public SkipListNode(TNKey key, TNValue value)
            {
                keyValue = new SkipListKVPair<TNKey, TNValue>(key, value);
            }
        }


        /// <summary>
        /// 头节点
        /// </summary>
        private SkipListNode<TKey, TValue> head;
        private int count = 0;
        private const int MaxLevels = 32;
        public SkipList()
        {
            head = new SkipListNode<TKey, TValue>();
            count = 0;
        }

        public TValue this[TKey key]
        {
            get
            {
                return get(key);
            }
            set
            {
                Add(key, value);
            }
        }

        private TValue get(TKey key)
        {
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);
            if (!found)
                throw new KeyNotFoundException("Unable to find entry with key \"" + key.ToString() + "\"");
            return position.Value;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> mkeys = new List<TKey>();
                walkEntries(item => mkeys.Add(item.Key));
                return mkeys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> mValues = new List<TValue>();
                walkEntries(item => mValues.Add(item.Value));
                return mValues;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);
            Console.WriteLine("Add {0} ,{1}", key, value);
            Console.WriteLine("searched {0} ,{1},{2}", position.Key, position.Value, found);

            if (found)
            {
                position.Value = value;
            }
            else
            {
                SkipListNode<TKey, TValue> newNode = new SkipListNode<TKey, TValue>(key, value);

                //设置节点自身的信息
                newNode.Left = position;
                newNode.Right = position.Right;

                //设置关联节点的信息
                position.Right = newNode;
                if (newNode.Right != null)
                {
                    newNode.Right.Left = newNode;
                }
                count++;

                promote(newNode);
            }
        }

        private void promote(SkipListNode<TKey, TValue> node)
        {
            int levels = randomLevels();
            SkipListNode<TKey, TValue> up = node.Left;
            SkipListNode<TKey, TValue> last = node;
            for (int i = levels; i > 0; i--)
            {
                while (up.Up == null && !up.isFront)
                    up = up.Left;
                if (up.isFront && up.Up == null)
                {
                    //头结点也缺少
                    up.Up = new SkipListNode<TKey, TValue>();
                    up.Up.Down = up;
                }
                //当前Up设置为上一层链表前置节点
                up = up.Up;

                SkipListNode<TKey, TValue> newNode = new SkipListNode<TKey, TValue>(last.keyValue);
                //新节点前后节点设置
                newNode.Left = up;
                newNode.Right = up.Right;

                //其他节点 和新节点关联关系
                up.Right = newNode;
                if (newNode.Right != null)
                {
                    newNode.Right.Left = newNode;
                }

                newNode.Down = last;
                newNode.Down.Up = newNode;

                last = newNode;
            }
        }

        /// <summary>
        /// 获取随机长度
        /// </summary>
        /// <returns></returns>
        private int randomLevels()
        {
            Random rand = new Random();
            int len = 0;
            while (rand.NextDouble() < 0.5f)
            {
                len++;
                if (len >= MaxLevels)
                { break; }
            }
            return len;
        }

        /// <summary>
        /// 跳表搜索
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="position">返回跳表插入位置</param>
        /// <returns>是否查找到节点，节点是否存在，存在返回true,否则返回false</returns>
        private bool search(TKey key, out SkipListNode<TKey, TValue> position)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            SkipListNode<TKey, TValue> currect;
            position = currect = head;

            while (true)
            {
                if (currect.Up == null)
                    break;
                currect = currect.Up;
            }

            bool isFindKey = false;
            while (true)
            {
                if (currect.Right == null || key.CompareTo(currect.Right.keyValue.Key) < 0)
                {
                    if (currect.Down == null)
                    {
                        position = currect;
                        isFindKey = false;
                        break;
                    }
                    currect = currect.Down;
                }
                else if (key.CompareTo(currect.Right.keyValue.Key) > 0)
                {
                    currect = currect.Right;
                }
                else if (key.CompareTo(currect.Right.keyValue.Key) == 0)
                {
                    currect = currect.Right;
                    while (currect.Down != null)
                    {
                        currect = currect.Down;
                    }
                    position = currect;
                    isFindKey = true;
                    break;
                }
            }
            return isFindKey;
        }
        private void walkEntries(Action<SkipListNode<TKey, TValue>> lambda)
        {
            SkipListNode<TKey, TValue> node = head;
            while (node.Right != null)
            {
                node = node.Right;
                lambda(node);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            head = new SkipListNode<TKey, TValue>();
            count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            bool found = search(key, out _);
            return found;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (array.IsReadOnly)
                throw new ArgumentException("The array argument is Read Only and new items cannot be added to it.");
            if (array.IsFixedSize && array.Length < count + index)
                throw new ArgumentException("The array argument does not have sufficient space for the SkipList entries.");

            int i = index;
            walkEntries(n => array[i++] = new KeyValuePair<TKey, TValue>(n.Key, n.Value));
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            SkipListNode<TKey, TValue> position = head;
            while (position.Right != null)
            {
                position = position.Right;
                yield return new KeyValuePair<TKey, TValue>(position.Key, position.Value);
            }
        }

        public bool Remove(TKey key)
        {
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);

            Console.WriteLine("Remove {0}", key);
            Console.WriteLine("searched {0} ,{1},{2}", position.Key, position.Value, found);

            if (found)
            {


                SkipListNode<TKey, TValue> DeleteNode = position;
                while (true)
                {
                    if (DeleteNode == null)
                    {
                        break;
                    }
                    //双向指定
                    DeleteNode.Left.Right = DeleteNode.Right;
                    if (DeleteNode.Right != null)
                    {
                        DeleteNode.Right.Left = DeleteNode.Left;
                    }

                    DeleteNode = DeleteNode.Up;
                }
                count--;
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            try
            {
                value = get(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = default(TValue);
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public override string ToString()
        {
            SkipListNode<TKey, TValue> frontNode = head;
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                if (frontNode == null)
                    break;
                SkipListNode<TKey, TValue> currNode = frontNode.Right;
                stringBuilder.Append("(front)->");
                while (currNode != null)
                {
                    stringBuilder.Append("(");
                    stringBuilder.Append(currNode.Key);
                    stringBuilder.Append(",");
                    stringBuilder.Append(currNode.Value);
                    stringBuilder.Append(")");
                    stringBuilder.Append("->");
                    currNode = currNode.Right;
                }
                stringBuilder.Append("nil\n");
                frontNode = frontNode.Up;
            }
            return stringBuilder.ToString();
        }
    }
}
