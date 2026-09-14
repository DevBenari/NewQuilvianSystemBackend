# Kamus Data — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Blueprint | `PLT-BP-001` revisi 1 |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Backend SHA | `f0d6855` |

---

## Kolom warisan yang tidak diulang per tabel

Seluruh tabel mewarisi `IdentityModel` dan karena itu memiliki sepuluh kolom berikut. Kolom-kolom
ini **tidak** diulang pada tabel mana pun di bawah:

`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`,
`CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`.

---

## Ringkasan tabel

| Tabel | Status | Modul pemilik | Catatan |
| --- | --- | --- | --- |
| `NumNumberSeries` | **`Baru`** | Platform | Pencacah deret nomor bersama |
| `BilNumberSeries` | `Sudah ada` | Billing | **Tidak disentuh.** Dicatat di sini hanya sebagai pembanding |

---

## `NumNumberSeries` — `Baru`

Satu baris menyimpan nilai terakhir **satu deret pada satu scope**. Baris lahir sendiri saat
alokasi pertama; tidak ada layar yang membuatnya.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | Tidak | — |
| `SequenceKey` | `string(50)` | Ya | — | Unik bersama `ScopeKey` | — | Tidak | Penanda deret, ditetapkan modul pemilik. Contoh `BBK_BLOOD_ORDER` |
| `ScopeKey` | `string(50)` | Ya | — | Unik bersama `SequenceKey` | — | Tidak | Periode berlakunya pencacah. `GLOBAL` bila tidak pernah diulang; `2026`, `202609`, atau `20260909` bila diulang |
| `ResetPolicy` | `string(20)` | Ya | — | — | — | Tidak | `NEVER`, `YEARLY`, `MONTHLY`, atau `DAILY`. Dijaga check constraint |
| `CurrentValue` | `long` | Ya | — | — | — | Tidak | Nilai terakhir yang **sudah terbit**. Dijaga check constraint `> 0` |
| `LastAllocatedAt` | `DateTimeOffset` | Ya | — | — | — | Tidak | Waktu alokasi terakhir. Dibaca layar pemantauan |

### Index dan constraint

| Nama | Jenis | Isi | Kenapa |
| --- | --- | --- | --- |
| `IX_NumNumberSeries_SequenceKey_ScopeKey` | **Unik** | `(SequenceKey, ScopeKey)` | Penjaga terakhir agar satu deret pada satu periode tidak punya dua pencacah. Tanpa ini, dua baris kembar akan menerbitkan nomor kembar |
| `CK_NumNumberSeries_CurrentValue` | Check | `"CurrentValue" > 0` | Pencacah bernilai nol berarti belum pernah terbit, dan barisnya seharusnya belum ada |
| `CK_NumNumberSeries_ResetPolicy` | Check | `IN ('NEVER','YEARLY','MONTHLY','DAILY')` | Kebijakan di luar keempatnya membuat scope key tidak dapat dihitung |

### Perilaku hapus

**Tidak ada.** Baris deret tidak pernah dihapus dan tidak pernah di-soft-delete lewat jalur
aplikasi mana pun. Menghapusnya berarti kehilangan nilai tertinggi yang pernah terbit, dan
alokasi berikutnya akan mengulang nomor yang sudah menempel pada catatan — pelanggaran langsung
`INV-PLT-001`.

Kesepuluh kolom warisan tetap ada karena diwarisi `IdentityModel`, tetapi `IsDelete` dan
`IsCancel` **tidak pernah** disetel `true` oleh kode mana pun pada slice ini.

### Concurrency

Dijaga `pg_advisory_xact_lock(hashtext(kunci))` di dalam transaksi alokasi, **bukan** oleh kolom
token versi. Alasannya: yang perlu diserialkan adalah *urutan pengambilan nomor*, bukan deteksi
tabrakan setelah kejadian. Token versi akan memulangkan kegagalan kepada pemanggil dan menuntut
percobaan ulang; kunci penasihat justru mengantre dan menyelesaikannya.

---

## `BilNumberSeries` — `Sudah ada`, milik Billing

Dicatat sebagai pembanding. **Slice ini tidak menyentuhnya.**

| Kolom kunci | Tipe | Keterangan |
| --- | --- | --- |
| `Id` | `Guid` | PK |
| `SequenceKey` | `string(50)` | Dipatok empat nilai `BILLING_*` |
| `ScopeKey` | `string(50)` | Unik bersama `SequenceKey` |
| `ResetPolicy` | `string(20)` | Sama empat nilainya |
| `CurrentValue` | `long` | **Perilaku berbeda:** kenaikannya ikut batal bila transaksi bisnis batal (`CONF-PLT-002`) |

Berkas model: `Areas/HealthServices/BillingManagement/Billing/Models/BilNumberSeries.cs`.

**Kenapa bentuknya sengaja dibuat sama.** `PLT-SLICE-02` kelak memindahkan empat deret Billing ke
tabel platform. Menjaga kolomnya identik membuat pemindahan itu menjadi penyalinan baris, bukan
penerjemahan bentuk.
