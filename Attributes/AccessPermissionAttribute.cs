using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Filters;

namespace QuilvianSystemBackend.Attributes
{
    /// <summary>
    /// Menjaga satu aksi controller dengan pasangan <c>resource : action</c> pada registry hak akses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pesan penolakan boleh dikhususkan, keputusan penolakan tidak.</b>
    /// <see cref="DeniedCode"/> dan <see cref="DeniedMessage"/> hanya mengganti <i>isi balasan</i>
    /// ketika akses sudah diputuskan ditolak oleh <see cref="AccessPermissionFilter"/>. Keduanya
    /// tidak pernah ikut menentukan boleh atau tidaknya seseorang masuk, sehingga tidak ada
    /// pemeriksaan hak akses kedua yang dapat menyimpang dari yang pertama.
    /// </para>
    /// <para>
    /// <b>Keduanya opsional dan bersifat opt-in.</b> Pemakaian lama
    /// <c>[AccessPermission("X", "Y")]</c> tetap sah dan tetap menghasilkan balasan generic yang
    /// sama persis seperti sebelumnya. Nilai yang tidak diisi dikirim sebagai string kosong,
    /// bukan <c>null</c>, supaya pemilihan konstruktor filter oleh DI tidak bergantung pada
    /// pencocokan tipe terhadap <c>null</c>.
    /// </para>
    /// <para>
    /// Dipakai ketika <c>contracts/validation-matrix.md</c> menuntut kode dan kalimat penolakan
    /// tertentu untuk satu endpoint — misalnya <c>VAL-BD-078</c> pada pencatatan bukti kecocokan
    /// Bank Darah.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class AccessPermissionAttribute : TypeFilterAttribute
    {
        private readonly string _controllerName;
        private readonly string _actionName;
        private string _deniedCode = string.Empty;
        private string _deniedMessage = string.Empty;

        public AccessPermissionAttribute(string controllerName, string actionName)
            : base(typeof(AccessPermissionFilter))
        {
            _controllerName = controllerName;
            _actionName = actionName;

            RefreshArguments();
        }

        /// <summary>
        /// Kode penolakan pada <c>validation-matrix.md</c>, misalnya <c>VAL-BD-078</c>.
        /// Kosong berarti balasan generic seperti sebelumnya.
        /// </summary>
        public string DeniedCode
        {
            get => _deniedCode;
            set
            {
                _deniedCode = value ?? string.Empty;
                RefreshArguments();
            }
        }

        /// <summary>
        /// Kalimat penolakan persis seperti yang diminta kontrak.
        /// Kosong berarti memakai kalimat generic bawaan filter.
        /// </summary>
        public string DeniedMessage
        {
            get => _deniedMessage;
            set
            {
                _deniedMessage = value ?? string.Empty;
                RefreshArguments();
            }
        }

        /// <remarks>
        /// Properti attribute diisi <i>setelah</i> konstruktor berjalan, sehingga
        /// <see cref="TypeFilterAttribute.Arguments"/> perlu disusun ulang setiap kali salah satu
        /// nilainya berubah.
        /// </remarks>
        private void RefreshArguments()
        {
            Arguments = new object[]
            {
                _controllerName,
                _actionName,
                _deniedCode,
                _deniedMessage
            };
        }
    }
}
