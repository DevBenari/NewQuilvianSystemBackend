# Integration Contract — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Owner | Pemilik kontrak engineering backend — `Andry` |
| Traceability | `DEC-PLT-003`, `DEC-PLT-005`, `DEC-PLT-007`, `INV-PLT-003`, `FACT-PLT-006` |

---

## 1. Siapa yang memanggil, dan bagaimana

Integrasi modul ini seluruhnya **dalam proses** — nol HTTP, nol pesan asinkron, nol pemanggilan
lintas jaringan.

| Konsumen | Deret yang direncanakan | Keadaan |
| --- | --- | --- |
| Bank Darah (`BD-BP-001`) | `OrderNumber`, `RequestNumber`, `ProcedureNumber` | **Menunggu slice ini** — gerbang `G4`, sembilan task backend tertahan |
| Billing & Kasir | Empat deret `BILLING_*` | **Tidak pindah pada slice ini** — lingkup `PLT-SLICE-02` |
| Rawat Inap | Direncanakan (`FACT-PLT-006`) | Belum dijadwalkan |
| Rawat Jalan | Direncanakan (`FACT-PLT-006`) | Belum dijadwalkan |

### Kontrak pemakaian

```csharp
public sealed class BbkBloodOrderService
{
    private readonly NumberSeriesAllocator _numberSeries;   // di-inject

    // dipanggil di dalam pekerjaan penyimpanan order
    var orderNumber = await _numberSeries.AllocateAsync(
        new NumberAllocationRequest(
            SequenceKey:     "BBK_BLOOD_ORDER",   // milik Bank Darah
            Prefix:          options.Prefix,       // milik Bank Darah
            ResetPolicy:     "NEVER",              // DEC-PLT-004
            SequenceDigits:  options.Digits,       // milik Bank Darah
            ActorUserId:     actorUserId,
            Instant:         DateTimeOffset.UtcNow),
        cancellationToken);
```

**Pembagian kewenangan yang terlihat langsung di tanda tangan method.** Empat dari enam parameter
datang dari modul pemanggil. Platform hanya menyediakan mesinnya. Inilah `DEC-PLT-005` dalam
bentuk kode.

---

## 2. Aturan yang mengikat konsumen

| Aturan | Isi | Akibat bila dilanggar |
| --- | --- | --- |
| **Satu deret satu pemilik** | `SequenceKey` dimiliki tepat satu modul. Dua modul **MUST NOT** memakai penanda yang sama | Dua modul berbagi pencacah; nomor keduanya saling menyela |
| **Alokasi hanya dari service** | Controller **MUST NOT** memanggil alokator (`QBE-CODE-002`) | Nomor terbit di luar pekerjaan yang menyimpan catatan |
| **Alokasi sedekat mungkin dengan penyimpanan** | Panggil ketika catatan sudah pasti akan disimpan, bukan di awal validasi | Setiap penolakan yang terjadi setelah alokasi menghanguskan satu nomor |
| **Nomor yang sudah terbit tidak diminta ulang** | Simpan hasilnya; jangan memanggil alokator dua kali untuk catatan yang sama | Deret berlubang tanpa sebab, dan catatan bisa punya dua nomor |
| **`ResetPolicy = NEVER` untuk deret baru** | `DEC-PLT-004` | Deret terulang saat ganti periode; melanggar `INV-PLT-004` |
| **Satu deret satu mekanisme** | Deret yang sudah dilayani mesin lama **MUST NOT** sekaligus dilayani mesin platform | `INV-PLT-003`; nomor kembar terbit |

**Aturan ketiga layak diperhatikan pelaksana.** Karena nomor hangus saat pekerjaan batal
(`DEC-PLT-008`), memanggil alokator terlalu awal membuat deret berlubang setiap kali validasi
menolak. Itu tidak salah — lubang adalah keadaan sah — tetapi tidak perlu.

---

## 3. Ketergantungan runtime

| Ketergantungan | Kenapa dibutuhkan | Risiko |
| --- | --- | --- |
| **PostgreSQL** | `pg_advisory_xact_lock` adalah fungsi khusus PostgreSQL | Alokator tidak dapat berjalan di provider lain. Provider InMemory **tidak** dapat membuktikan durabilitas maupun antrean |
| **`IDbContextFactory<ApplicationDbContext>`** | Alokator membuka koneksi dan transaksinya sendiri agar pencacah bertahan (`DEC-PLT-008`) | Perlu didaftarkan berdampingan dengan `AddDbContext` yang sudah ada. **Wajib diverifikasi saat implementasi**, bukan dianggap pasti aman |
| Zona waktu bisnis | Penghitungan periode untuk kebijakan pengulangan selain `NEVER` | Mengikuti mesin yang ada: `Asia/Jakarta`, dengan cadangan `SE Asia Standard Time` |

---

## 4. Apa yang tidak berubah pada Billing

Dinyatakan tegas karena inilah kekhawatiran `NOTE-PLT-001`:

| Aspek Billing | Dampak slice ini |
| --- | --- |
| `BillingNumberSeriesService` | **Nol perubahan** |
| Tabel `BilNumberSeries` | **Nol perubahan** |
| Empat deret produksi — invoice, deposit, shift kasir, kwitansi | **Nol perubahan perilaku.** Tetap memakai ulang nomor saat transaksi batal |
| Empat pemanggilnya — `BillingInvoiceService`, `BillingDepositService`, `BillingSettlementService`, `CashierShiftService` | **Nol perubahan** |
| Rupa nomor yang sudah terbit | **Nol perubahan** |

Perubahan perilaku keempat deret itu adalah lingkup `PLT-SLICE-02`, dan menuntut `DEC-PLT-006`
dijawab lebih dulu — yaitu sampai kapan pelanggaran `INV-PLT-001` selama peralihan diterima
resmi.

---

## 5. Observability

`QBE-CODE-006` menuntut provider bersama menyediakan observability retry. Yang disediakan:

| Yang dicatat | Di mana | Sensitif |
| --- | --- | :---: |
| Deret, periode, nilai yang terbit, pelaku, waktu | Baris `NumNumberSeries` itu sendiri | Tidak |
| Kegagalan alokasi beserta kode validasinya | Log aplikasi, kategori `Platform.NumberSeries` | Tidak |
| Lama menunggu giliran pada deret yang ramai | Log aplikasi | Tidak |
| Layar pemantauan | `GET /number-series` — lihat `api-contract.md` | Tidak |

Nol kolom pada modul ini bertanda sensitif; nomor bisnis bukan data pribadi.
