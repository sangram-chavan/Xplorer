using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Xplorer.Core.ThreadSafeUtilities
{
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        //This is the internal dictionary that we are wrapping
        protected readonly IDictionary<TKey, TValue> _dict;


        [NonSerialized]
        private readonly ReaderWriterLockSlim _dictionaryLock =
            Locks.GetLockInstance(LockRecursionPolicy.NoRecursion); //setup the lock;

        public ThreadSafeDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dict = new Dictionary<TKey, TValue>(dictionary);
        }

        public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(comparer);
        }

        public ThreadSafeDictionary(int capacity)
        {
            _dict = new Dictionary<TKey, TValue>(capacity);
        }

        public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public ThreadSafeDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        #region IDictionary<TKey,TValue> Members

        public virtual bool Remove(TKey key)
        {
            //using (new WriteLock(this.dictionaryLock))
            //{
            //    return this.dict.Remove(key);
            //}

            using (new ReadLock(_dictionaryLock))
            {
                if (_dict.ContainsKey(key))
                {
                    using (new WriteLock(_dictionaryLock))
                    {
                        return _dict.Remove(key);
                    }
                }
            }
            return false;
        }


        public virtual bool ContainsKey(TKey key)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.ContainsKey(key);
            }
        }


        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.TryGetValue(key, out value);
            }
        }


        public virtual TValue this[TKey key]
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict[key];
                }
            }
            set
            {
                using (new WriteLock(_dictionaryLock))
                {
                    _dict[key] = value;
                }
            }
        }


        public virtual ICollection<TKey> Keys
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return new List<TKey>(_dict.Keys);
                }
            }
        }


        public virtual ICollection<TValue> Values
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return new List<TValue>(_dict.Values);
                }
            }
        }


        public virtual void Clear()
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Clear();
            }
        }


        public virtual int Count
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict.Count;
                }
            }
        }


        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.Contains(item);
            }
        }


        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Add(item);
            }
        }


        public virtual void Add(TKey key, TValue value)
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Add(key, value);
            }
        }


        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(_dictionaryLock))
            {
                return _dict.Remove(item);
            }
        }


        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                _dict.CopyTo(array, arrayIndex);
            }
        }


        public virtual bool IsReadOnly
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict.IsReadOnly;
                }
            }
        }


        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }

        #endregion
    }
    
    public class ThreadSafeSortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        //This is the internal dictionary that we are wrapping
        protected readonly IDictionary<TKey, TValue> _dict;


        [NonSerialized]
        private readonly ReaderWriterLockSlim _dictionaryLock =
            Locks.GetLockInstance(LockRecursionPolicy.NoRecursion); //setup the lock;

        public ThreadSafeSortedDictionary()
        {
            _dict = new SortedDictionary<TKey, TValue>();
        }

        public ThreadSafeSortedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dict = new SortedDictionary<TKey, TValue>(dictionary);
        }

        public ThreadSafeSortedDictionary(IComparer<TKey> comparer)
        {
            _dict = new SortedDictionary<TKey, TValue>(comparer);
        }

        public ThreadSafeSortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            _dict = new SortedDictionary<TKey, TValue>(dictionary, comparer);
        }

        #region IDictionary<TKey,TValue> Members

        public virtual bool Remove(TKey key)
        {
            //using (new WriteLock(this.dictionaryLock))
            //{
            //    return this.dict.Remove(key);
            //}

            using (new ReadLock(_dictionaryLock))
            {
                if (_dict.ContainsKey(key))
                {
                    using (new WriteLock(_dictionaryLock))
                    {
                        return _dict.Remove(key);
                    }
                }
            }
            return false;
        }


        public virtual bool ContainsKey(TKey key)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.ContainsKey(key);
            }
        }


        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.TryGetValue(key, out value);
            }
        }


        public virtual TValue this[TKey key]
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict[key];
                }
            }
            set
            {
                using (new WriteLock(_dictionaryLock))
                {
                    _dict[key] = value;
                }
            }
        }


        public virtual ICollection<TKey> Keys
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return new List<TKey>(_dict.Keys);
                }
            }
        }


        public virtual ICollection<TValue> Values
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return new List<TValue>(_dict.Values);
                }
            }
        }


        public virtual void Clear()
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Clear();
            }
        }


        public virtual int Count
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict.Count;
                }
            }
        }


        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                return _dict.Contains(item);
            }
        }


        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Add(item);
            }
        }


        public virtual void Add(TKey key, TValue value)
        {
            using (new WriteLock(_dictionaryLock))
            {
                _dict.Add(key, value);
            }
        }


        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (new WriteLock(_dictionaryLock))
            {
                return _dict.Remove(item);
            }
        }


        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (new ReadOnlyLock(_dictionaryLock))
            {
                _dict.CopyTo(array, arrayIndex);
            }
        }


        public virtual bool IsReadOnly
        {
            get
            {
                using (new ReadOnlyLock(_dictionaryLock))
                {
                    return _dict.IsReadOnly;
                }
            }
        }


        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }

        #endregion
    }

    public static class Locks
    {
        public static void GetReadLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterUpgradeableReadLock(1);
        }


        public static void GetReadOnlyLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterReadLock(1);
        }


        public static void GetWriteLock(ReaderWriterLockSlim locks)
        {
            bool lockAcquired = false;
            while (!lockAcquired)
                lockAcquired = locks.TryEnterWriteLock(1);
        }


        public static void ReleaseReadOnlyLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsReadLockHeld)
                locks.ExitReadLock();
        }


        public static void ReleaseReadLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsUpgradeableReadLockHeld)
                locks.ExitUpgradeableReadLock();
        }


        public static void ReleaseWriteLock(ReaderWriterLockSlim locks)
        {
            if (locks.IsWriteLockHeld)
                locks.ExitWriteLock();
        }


        public static void ReleaseLock(ReaderWriterLockSlim locks)
        {
            ReleaseWriteLock(locks);
            ReleaseReadLock(locks);
            ReleaseReadOnlyLock(locks);
        }


        public static ReaderWriterLockSlim GetLockInstance()
        {
            return GetLockInstance(LockRecursionPolicy.SupportsRecursion);
        }


        public static ReaderWriterLockSlim GetLockInstance(LockRecursionPolicy recursionPolicy)
        {
            return new ReaderWriterLockSlim(recursionPolicy);
        }
    }


    public abstract class BaseLock : IDisposable
    {
        protected ReaderWriterLockSlim Locks;


        protected BaseLock(ReaderWriterLockSlim locks)
        {
            Locks = locks;
        }

        #region IDisposable Members

        public abstract void Dispose();

        #endregion
    }


    public class ReadLock : BaseLock
    {
        public ReadLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            ThreadSafeUtilities.Locks.GetReadLock(Locks);
        }


        public override void Dispose()
        {
            ThreadSafeUtilities.Locks.ReleaseReadLock(Locks);
        }
    }


    public class ReadOnlyLock : BaseLock
    {
        public ReadOnlyLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            ThreadSafeUtilities.Locks.GetReadOnlyLock(Locks);
        }


        public override void Dispose()
        {
            ThreadSafeUtilities.Locks.ReleaseReadOnlyLock(Locks);
        }
    }


    public class WriteLock : BaseLock
    {
        public WriteLock(ReaderWriterLockSlim locks)
            : base(locks)
        {
            ThreadSafeUtilities.Locks.GetWriteLock(Locks);
        }


        public override void Dispose()
        {
            ThreadSafeUtilities.Locks.ReleaseWriteLock(Locks);
        }
    }
}