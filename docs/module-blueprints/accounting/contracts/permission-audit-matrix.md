# Accounting — Permission dan Audit Matrix

| Field | Value |
|---|---|
| `contract_version` | `ACC-PERMISSION-0.2` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner), owner keamanan platform |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3`, `02-backend-architecture.md@3` |
| Traceability | `ACC-DEC-015`, `ACC-DEC-016`, `ACC-DEC-026`, `ACC-DEC-027`, `ACC-DEC-031`, `ACC-DEC-032`, `ACC-DEC-033`, **`ACC-DEC-041`** |
| Dampak kompatibilitas | Menambah nilai `Resource` baru; tidak mengubah mekanisme hak akses yang ada |

Nilai `[AccessPermission(...)]` di bawah ditulis **apa adanya** agar implementer menyalin, bukan
menerjemahkan. Mekanisme hak akses memakai `[AccessController]`, `[AccessAction]`,
`[AccessPermission("Resource", "Action")]`, dan `AccessTypes` yang sudah ada di repository —
Accounting **tidak** membuat sistem hak akses tandingan.

## 1. Enam peran

`ACC-DEC-031`. Nama peran final mengikuti pendaftaran pada mekanisme hak akses yang berlaku.

| Peran | Tugas utamanya | Siapa orangnya |
|---|---|---|
| Accounting Viewer | Melihat jurnal dan laporan, tidak mengubah apa pun | Manajemen, unit terkait |
| Accounting Staff | Membuat dan mengajukan jurnal | Staf akuntansi |
| Accounting Approver | Menyetujui atau menolak jurnal | Supervisor akuntansi |
| Accounting Manager | Mengesahkan jurnal, membalik, menutup dan membuka periode | Kepala bagian akuntansi |
| Auditor | Membaca seluruh riwayat dan jejak audit, tanpa hak mengubah | Auditor internal dan eksternal |
| Accounting Administrator | Mengatur daftar akun, jenis jurnal, dan membangkitkan periode | Admin sistem akuntansi |

## 2. Matriks peran terhadap tindakan

Tanda centang berarti peran itu memiliki hak tersebut.

| Tindakan | String permission | Viewer | Staff | Approver | Manager | Auditor | Administrator |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Lihat daftar akun | `[AccessPermission("ChartOfAccount", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tambah akun | `[AccessPermission("ChartOfAccount", "Create")]` | | | | | | ✓ |
| Ubah akun | `[AccessPermission("ChartOfAccount", "Update")]` | | | | | | ✓ |
| Lihat jenis jurnal | `[AccessPermission("JournalType", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tambah jenis jurnal | `[AccessPermission("JournalType", "Create")]` | | | | | | ✓ |
| Ubah jenis jurnal | `[AccessPermission("JournalType", "Update")]` | | | | | | ✓ |
| Lihat jurnal | `[AccessPermission("Journal", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Buat jurnal | `[AccessPermission("Journal", "Create")]` | | ✓ | | ✓ | | |
| Ubah jurnal draft | `[AccessPermission("Journal", "Update")]` | | ✓ | | ✓ | | |
| Hapus jurnal draft | `[AccessPermission("Journal", "Delete")]` | | ✓ | | ✓ | | |
| Ajukan jurnal | `[AccessPermission("Journal", "Submit")]` | | ✓ | | ✓ | | |
| Setujui atau tolak jurnal | `[AccessPermission("Journal", "Approve")]` | | | ✓ | ✓ | | |
| Sahkan jurnal | `[AccessPermission("Journal", "Post")]` | | | | ✓ | | |
| Balik jurnal | `[AccessPermission("Journal", "Reverse")]` | | | | ✓ | | |
| Lihat periode | `[AccessPermission("AccountingPeriod", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Bangkitkan periode setahun | `[AccessPermission("AccountingPeriod", "Create")]` | | | | | | ✓ |
| Tutup periode | `[AccessPermission("AccountingPeriod", "Close")]` | | | | ✓ | | |
| Buka kembali periode | `[AccessPermission("AccountingPeriod", "Reopen")]` | | | | ✓ | | |
| Lihat buku besar dan neraca saldo | `[AccessPermission("GeneralLedger", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

Dua hal yang perlu diperhatikan pada matriks di atas:

1. **Accounting Manager memiliki hak Staff juga.** Ini disengaja agar Manager dapat membuat
   jurnal saat staf berhalangan. Namun `ACC-DEC-016` tetap berlaku penuh: Manager yang membuat
   sebuah jurnal **tidak** boleh menyetujui jurnal itu sendiri, walaupun ia punya hak
   `Journal : Approve`.
2. **Administrator tidak memegang hak transaksi.** Ia mengatur master, bukan mencatat jurnal.
   Pemisahan ini yang biasa diminta auditor.

## 3. Matriks endpoint terhadap permission dan pencatatan

`ACC-DEC-032` membatasi pencatatan pembacaan. Konvensi repository: permintaan `GET` tidak
dicatat logger. Accounting mengikutinya, dengan **dua pengecualian** yang ditandai tebal.

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /chart-of-accounts` | `ChartOfAccount` | `Read` | `[AccessPermission("ChartOfAccount", "Read")]` | Tidak |
| `GET /chart-of-accounts/{id}` | `ChartOfAccount` | `Read` | `[AccessPermission("ChartOfAccount", "Read")]` | Tidak |
| `GET /chart-of-accounts/tree` | `ChartOfAccount` | `Read` | `[AccessPermission("ChartOfAccount", "Read")]` | Tidak |
| `GET /chart-of-accounts/options` | `ChartOfAccount` | `Read` | `[AccessPermission("ChartOfAccount", "Read")]` | Tidak |
| `POST /chart-of-accounts` | `ChartOfAccount` | `Create` | `[AccessPermission("ChartOfAccount", "Create")]` | Ya |
| `PUT /chart-of-accounts/{id}` | `ChartOfAccount` | `Update` | `[AccessPermission("ChartOfAccount", "Update")]` | Ya |
| `PATCH /chart-of-accounts/{id}/deactivate` | `ChartOfAccount` | `Update` | `[AccessPermission("ChartOfAccount", "Update")]` | Ya |
| `GET /journal-types` | `JournalType` | `Read` | `[AccessPermission("JournalType", "Read")]` | Tidak |
| `POST /journal-types` | `JournalType` | `Create` | `[AccessPermission("JournalType", "Create")]` | Ya |
| `PUT /journal-types/{id}` | `JournalType` | `Update` | `[AccessPermission("JournalType", "Update")]` | Ya |
| `GET /journals` | `Journal` | `Read` | `[AccessPermission("Journal", "Read")]` | Tidak |
| `GET /journals/{id}` | `Journal` | `Read` | `[AccessPermission("Journal", "Read")]` | Tidak |
| `POST /journals` | `Journal` | `Create` | `[AccessPermission("Journal", "Create")]` | Ya |
| `PUT /journals/{id}` | `Journal` | `Update` | `[AccessPermission("Journal", "Update")]` | Ya |
| `DELETE /journals/{id}` | `Journal` | `Delete` | `[AccessPermission("Journal", "Delete")]` | Ya |
| `POST /journals/{id}/submit` | `Journal` | `Submit` | `[AccessPermission("Journal", "Submit")]` | Ya |
| `POST /journals/{id}/approve` | `Journal` | `Approve` | `[AccessPermission("Journal", "Approve")]` | Ya |
| `POST /journals/{id}/reject` | `Journal` | `Approve` | `[AccessPermission("Journal", "Approve")]` | Ya |
| `POST /journals/{id}/post` | `Journal` | `Post` | `[AccessPermission("Journal", "Post")]` | Ya |
| `POST /journals/{id}/reverse` | `Journal` | `Reverse` | `[AccessPermission("Journal", "Reverse")]` | Ya |
| `GET /periods` | `AccountingPeriod` | `Read` | `[AccessPermission("AccountingPeriod", "Read")]` | Tidak |
| `GET /periods/current` | `AccountingPeriod` | `Read` | `[AccessPermission("AccountingPeriod", "Read")]` | Tidak |
| `POST /periods/generate` | `AccountingPeriod` | `Create` | `[AccessPermission("AccountingPeriod", "Create")]` | Ya |
| `POST /periods/{id}/close` | `AccountingPeriod` | `Close` | `[AccessPermission("AccountingPeriod", "Close")]` | Ya |
| `POST /periods/{id}/reopen` | `AccountingPeriod` | `Reopen` | `[AccessPermission("AccountingPeriod", "Reopen")]` | Ya |
| `GET /general-ledger/movements` | `GeneralLedger` | `Read` | `[AccessPermission("GeneralLedger", "Read")]` | Tidak |
| **`GET /general-ledger/trial-balance`** | `GeneralLedger` | `Read` | `[AccessPermission("GeneralLedger", "Read")]` | **Ya** |
| `GET /general-ledger/account-balance/{accountId}` | `GeneralLedger` | `Read` | `[AccessPermission("GeneralLedger", "Read")]` | Tidak |

### Kenapa hanya neraca saldo yang pembacaannya dicatat

`ACC-DEC-032` memilih mencatat pembacaan **laporan keuangan** dan **riwayat pemetaan posting**.
Pada MVP, satu-satunya laporan keuangan yang ada adalah Neraca Saldo (`ACC-DEC-030`), dan
pemetaan posting belum dibangun karena ada di Phase 2. Karena itu hanya satu endpoint yang
dicatat pembacaannya.

Alasan tidak mencatat semuanya: satu pengguna dapat membuka Buku Besar 200 kali sehari saat
menelusuri selisih. Mencatat semuanya akan menghasilkan 200 baris yang tidak berguna dan justru
menyulitkan penelusuran saat benar-benar dibutuhkan.

**Saat Phase 2 berjalan**, endpoint pembacaan riwayat pemetaan posting wajib ditambahkan ke
daftar yang dicatat, dan Laba Rugi serta Neraca menyusul begitu keduanya dibangun.

## 4. Isi catatan logger

| Yang dicatat | Yang **TIDAK** boleh dicatat |
|---|---|
| `EntityId`, nama controller, nama action, hasil (berhasil atau gagal), pengguna, waktu | Nilai `TotalDebit`, `TotalCredit`, `DebitAmount`, `CreditAmount` |
| Perubahan status jurnal, misalnya dari `Approved` ke `Posted` | Isi `Description` jurnal maupun baris jurnal |
| Alasan penutupan dan pembukaan kembali periode | — |

Kolom bertanda **Sensitif** pada [../erd/data-dictionary.md](../erd/data-dictionary.md) tidak
boleh masuk payload log. Untuk Accounting, yang sensitif adalah **rahasia bisnis** berupa nilai
uang dan keterangan jurnal — bukan data pribadi, karena MVP tidak menyimpan satu pun kolom
pasien maupun pegawai.

## 5. Aturan hak akses yang tidak dapat diwakili matriks

Tiga aturan berikut bergantung pada **data**, bukan hanya pada peran, sehingga wajib ditegakkan
di dalam service.

| Aturan | Keputusan asal | Tempat penegakan |
|---|---|---|
| Penyetuju tidak boleh sama dengan pembuat jurnal | `ACC-DEC-016` | `AccJournalService`, membandingkan pengguna yang sedang masuk dengan `CreateBy` jurnal |
| ~~Pengguna hanya boleh menyentuh badan hukum yang menjadi haknya~~ **`DEFERRED`** | `ACC-DEC-037`, ditunda oleh **`ACC-DEC-041`** | **Belum ditegakkan.** Digantikan sementara oleh penjaga jumlah badan hukum — lihat blok di bawah |
| Saldo awal disahkan Manager dengan persetujuan pimpinan keuangan | `ACC-DEC-033` | Jenis jurnal `SA` menuntut persetujuan, dan `Journal : Post` hanya dimiliki Manager |

### Aturan kedua ditunda `ACC-DEC-041` — dan apa penggantinya

Versi `0.1` menyatakan: *"penyaringan badan hukum bukan urusan tampilan; backend wajib menolak
permintaan atas badan hukum yang bukan hak pengguna."* Kalimat itu **masih benar sebagai target**,
tetapi **tidak dapat dipenuhi** — `BE-ACC-002` membuktikan mekanismenya tidak ada di platform, dan
`ACC-DEP-008` tidak akan dikerjakan dalam waktu dekat.

Diverifikasi 2 September 2026: **17** controller menerima `LegalEntityId` dari `[FromQuery]`,
**0** klaim badan hukum di JWT, **0** `HasQueryFilter` di seluruh repository. `LegalEntityId`
selalu datang dari pengirim permintaan, tidak pernah dari identitas.

Karena itu `ACC-DEC-041` menurunkan MVP menjadi **satu badan hukum**, dan aturan ini ditunda
sampai `ACC-DEP-008` selesai. **Penggantinya wajib ada, bukan opsional:**

> **Penjaga jumlah badan hukum.** Bila `MstLegalEntity` yang `IsActive` dan tidak terhapus
> berjumlah **lebih dari satu**, endpoint Accounting menolak permintaan dan menyebutkan bahwa
> penyaringan badan hukum per pengguna belum tersedia.

Penjaga ini **bukan** sistem hak akses tandingan — ia tidak menentukan siapa berhak atas apa,
hanya menolak berjalan pada keadaan yang belum dapat dijaga. Ditempatkan di `BE-ACC-007` sebagai
acceptance tersendiri.

**Yang TIDAK ditunda:** pemisahan data tetap berlaku penuh. Kode akun tetap unik per badan hukum,
dan satu jurnal tetap tidak boleh mencampur dua badan hukum (`BE-ACC-010`). `ACC-DEC-037` tidak
dibatalkan.
