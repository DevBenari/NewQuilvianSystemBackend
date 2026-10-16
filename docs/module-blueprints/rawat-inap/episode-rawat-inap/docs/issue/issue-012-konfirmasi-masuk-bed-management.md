# ISSUE-012 — Ketiadaan State Loading dan Modal Detail Bed Tidak Tertutup Otomatis Pasca Konfirmasi Masuk

```yaml
issue_id: ISSUE-EPS-012
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Bed Management — Modal Detail Tempat Tidur & Dialog Konfirmasi Masuk (FE-BM-01, FE-BM-04)"
sumber_laporan: "Laporan pemilik sistem 13-10-2026: ketiadaan visual state loading saat konfirmasi masuk dan modal detail bed tidak tertutup otomatis setelah konfirmasi sukses"
tanggal_issue: "2026-10-13"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "a1569f4787d98476b73ba5f8694e1eb73bc03588 (MHamzah)"
source_sha_frontend: "9d890cf1c6b298e57cf79de8181012405972755e (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-012-konfirmasi-masuk-bed-management.md
```

## 1. Ringkasan

Pada antarmuka Bed Management (`/health-services/inpatient-management/bed-board`), ketika petugas mengeklik kartu tempat tidur yang berstatus *Dipesan* (misalnya BED 001), sistem membuka modal pop-up **Detail Tempat Tidur**. Di dalam modal tersebut terdapat tombol aksi utama **"Konfirmasi Masuk"** dan tombol sekunder **"Batalkan Pesanan"**. 

Ketika tombol "Konfirmasi Masuk" diklik, sistem tidak memberikan indikator pemuatan visual (*spinner* atau teks proses) pada tombol tersebut saat melakukan pembacaan ulang data ke server (*prepare*). Tombol hanya sekadar dinonaktifkan (*disabled*) tanpa animasi, sehingga petugas merasa bimbang apakah sistem sedang memproses ("state loading") atau tombol tidak merespons. Lebih lanjut, setelah petugas mengonfirmasi penempatan pasien melalui dialog penegas dan transaksi berhasil dicatat di server (*placement success*), modal pop-up **Detail Tempat Tidur tidak tertutup secara otomatis**. Modal induk tersebut tetap menggantung di layar dengan data usang (*stale state* yang masih berstatus "Dipesan" beserta tombol aksinya), memaksa petugas menutup modal secara manual dan memicu risiko klik ganda yang dapat memunculkan galat konflik versi (*HTTP 409 Conflict*). Masalah ini diklasifikasikan sebagai **High** karena mengganggu alur kerja pelayanan operasional rawat inap dan berpotensi memicu anomali integritas penempatan pasien.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "Belum ada state loading atau sudah keluar ketika saya klik konfirmasi masuk sudah berhasil modal ya tidak tertutup secara otomatis , coba anda analisis masalah tersebut" | Screenshot modal "Detail Tempat Tidur: BED 001" (Kode Bed BD-RSMMC-00041, Ruang Rawat Inap Kelas II 2, Status Dipesan), menampilkan tombol "Konfirmasi Masuk" dan "Batalkan Pesanan", serta riwayat pembersihan di bawahnya. |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-012-01` | 1 | Ketiadaan indikator visual pemuatan (*loading state*) pada tombol "Konfirmasi Masuk" dan "Batalkan Pesanan" di modal Detail Bed | `BUG`, `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI di `bed-management-view.jsx:329-348` | `FIX-EPS-012-01` |
| `ISS-EPS-012-02` | 1 | Modal Detail Tempat Tidur tidak tertutup secara otomatis setelah transaksi konfirmasi masuk berhasil | `BUG` | Frontend | High | SUDAH-VERIFIKASI di `bed-management-view.jsx:36-37` dan `use-inpatient-bed-board-actions.jsx:107-118` | `FIX-EPS-012-02` |
| `ISS-EPS-012-T1` | — | Potensi penolakan server (*HTTP 409 Conflict*) akibat data tempat tidur usang (*stale snapshot*) jika modal tidak tertutup dan diklik kembali | `BUG` | Frontend | High | SUDAH-VERIFIKASI di `bed-management-view.jsx:288-356` | `FIX-EPS-012-02` |
| `ISS-EPS-012-T2` | — | Tombol "Batalkan Pesanan" mengalami perilaku identik (tidak ada loading prepare dan modal tidak tertutup otomatis pasca-batal sukses) | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI di `use-inpatient-bed-board-actions.jsx:108-118` | `FIX-EPS-012-01`, `FIX-EPS-012-02` |

---

## 4. Rincian per temuan

### ISS-EPS-012-01 — Ketiadaan indikator visual pemuatan (*loading state*) pada tombol "Konfirmasi Masuk" di modal Detail Bed

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG`, `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source `bed-management-view.jsx:329-336` |
| **Perbaikan** | `FIX-EPS-012-01` |

**Apa yang terjadi.** Petugas rawat inap yang membuka rincian bed dan mengeklik tombol "Konfirmasi Masuk" tidak melihat adanya animasi putaran (*spinner*) atau label teks sedang memuat. Tombol hanya tampak sedikit meredup karena atribut `disabled`, sehingga timbul kebingungan apakah sistem sedang memproses ("ada state loading") ataukah aksi tersebut gagal berjalan / macet.

**Kenapa terjadi.** Pada berkas `bed-management-view.jsx`, tombol "Konfirmasi Masuk" dirender menggunakan komponen `BaseButton` tanpa menyematkan prop `loading`:

```jsx
// c:\Users\Asoiii\Documents\Quilvian\SourceCode\QuilvianFinal\QuilvianSystemFrontendDev\src\components\view\health-services\inpatient-management\bed-management\bed-management-view.jsx:328-336
{actions.canPlace ? (
  <BaseButton
    disabled={busy}
    onClick={() =>
      actions.requestConfirmAdmission(selectedBed)
    }
  >
    Konfirmasi Masuk
  </BaseButton>
) : null}
```

Ketika tombol diklik, fungsi `actions.requestConfirmAdmission(selectedBed)` mengeksekusi fungsi asinkron `prepare(bed, false)` pada hook `useInpatientBedBoardActions.jsx:26-77`. Fungsi tersebut menyetel `setPreparing(true)` lalu memanggil `await refresh?.()` ke backend untuk memvalidasi versi tempat tidur terbaru (*operationalVersion*). Selama pemanggilan jaringan ini berlangsung, nilai `actions.preparing` bernilai `true` sehingga variabel `busy` bernilai `true`. Karena `BaseButton` hanya diberi `disabled={busy}` tanpa `loading={actions.preparing}`, tombol hanya dinonaktifkan tanpa visualisasi spinner. Hal ini melanggar panduan interaktivitas komponen dasar Quilvian di mana aksi asinkron wajib memberikan umpan balik visual (*loading feedback*).

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG` dan `RULE_VIOLATION` terhadap standar UI Quilvian (`base-component-catalog.md`), di mana tombol aksi yang memicu proses I/O atau penyiapan asinkron wajib memiliki status `loading` agar pengguna mengetahui proses sedang berjalan.

**Dampak nyata.** Petugas loket rawat inap merasa sistem tidak merespons setelah mengeklik tombol, lalu mencoba mengeklik berulang kali secara tergesa-gesa atau merasa ragu apakah aksi sudah tercatat.

**Rekomendasi.** Sematkan prop `loading={actions.preparing && actions.activeAction === "confirm"}` (atau setidaknya `loading={actions.preparing}`) dan `disabled={busy}` pada tombol "Konfirmasi Masuk" dan "Batalkan Pesanan", sehingga saat proses pembacaan data berjalan muncul animasi spinner resmi dari `BaseButton`.

---

### ISS-EPS-012-02 — Modal Detail Tempat Tidur tidak tertutup secara otomatis setelah transaksi konfirmasi masuk berhasil

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source `bed-management-view.jsx:36-37` & `use-inpatient-bed-board-actions.jsx:107-118` |
| **Perbaikan** | `FIX-EPS-012-02` |

**Apa yang terjadi.** Setelah petugas mengeklik "Konfirmasi Masuk" pada dialog konfirmasi penegas `ConfirmModal`, request penempatan pasien (`POST /placements`) berhasil diproses server, dan notifikasi hijau (*Toast*) muncul di layar. Namun, modal induk pop-up **Detail Tempat Tidur: BED 001** tetap terbuka di layar. Petugas harus mengarahkan kursor dan mengeklik tombol "Tutup" atau tombol silang (X) secara manual.

**Kenapa terjadi.** Alur penutupan modal pada kode frontend terputus di antara dua lapisan komponen:
1. Modal induk dikendalikan oleh state lokal `selectedBed` pada `BedManagementSession` (`bed-management-view.jsx:37`):
   ```javascript
   const [selectedBed, setSelectedBed] = useState(null);
   ```
2. Hook pengendali aksi `useInpatientBedBoardActions` diinisialisasi tanpa callback penutupan modal:
   ```javascript
   const actions = useInpatientBedBoardActions({ refresh: state.refresh });
   ```
3. Saat penempatan pasien berhasil dijalankan pada `useInpatientBedBoardActions.jsx:107-118`:
   ```javascript
   await operation.run(body, ...);
   setConfirmTarget(null); // Hanya menutup ConfirmModal sekunder!
   setCancelTarget(null);
   setFailure(null);
   setToasts(...);
   await refresh?.();
   ```
   Fungsi `submit()` hanya memanggil `setConfirmTarget(null)` yang bertugas menutup dialog penegas `ConfirmModal`. Hook ini **tidak memiliki akses** dan **tidak memanggil callback apapun** untuk menutup modal induk `selectedBed` (`setSelectedBed(null)` tidak pernah dipanggil).
4. Akibatnya, modal `selectedBed` tetap terbuka dengan data snapshot lama (`selectedBed.statusLabel = "Dipesan"`).

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG`. Pada alur kerja operasional rumah sakit, setelah suatu tindakan berhasil dieksekusi tuntas, modal operasional transaksi wajib ditutup otomatis dan mengembalikan fokus pengguna ke papan monitoring utama yang sudah menampilkan status teranyar.

**Dampak nyata.** 
1. Petugas bingung apakah pasien sudah benar-benar masuk atau belum, karena di layar masih tertulis "BED 001 — Dipesan".
2. Menghambat kecepatan pelayanan pasien di loket rawat inap karena petugas harus menutup modal secara manual setiap kali selesai melakukan admisi/penempatan.

**Rekomendasi.** 
Tambahkan parameter callback `onSuccess` pada hook `useInpatientBedBoardActions`:
```javascript
const actions = useInpatientBedBoardActions({
  refresh: state.refresh,
  onSuccess: () => setSelectedBed(null),
});
```
Pada fungsi `submit` di dalam hook, panggil `onSuccess?.()` tepat setelah operasi berhasil (`await operation.run(...)`). Dengan demikian, modal induk `selectedBed` otomatis tertutup berbarengan dengan munculnya notifikasi toast sukses.

---

### ISS-EPS-012-T1 — Potensi penolakan server (*HTTP 409 Conflict*) akibat data tempat tidur usang (*stale snapshot*)

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source `bed-management-view.jsx:288-356` |
| **Perbaikan** | `FIX-EPS-012-02` |

**Apa yang terjadi.** Karena modal `selectedBed` tidak tertutup otomatis dan state objek `selectedBed` adalah objek statis yang disimpan sejak pertama kali modal dibuka, tampilan modal tetap menampilkan tombol "Konfirmasi Masuk". Jika petugas yang bingung mengeklik tombol "Konfirmasi Masuk" untuk kedua kalinya, sistem akan mencoba memvalidasi dan mengirimkan permintaan penempatan menggunakan nomor versi tempat tidur yang lama.

**Kenapa terjadi.** `refresh?.()` memperbarui koleksi `state.monitoring`, tetapi tidak memperbarui variabel state lokal `selectedBed`. Objek `selectedBed` masih membawa `expectedBedVersion` sebelum penempatan. Saat request kedua dieksekusi, backend melempar galat konkurensi optimistik (`409 Conflict` atau `ExpectedBedVersion mismatch`) yang memunculkan peringatan merah di antarmuka.

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG` turunan dari tidak tertutupnya modal induk.

**Dampak nyata.** Petugas rawat inap melihat pesan error penolakan transaksi ("Konfirmasi Masuk Ditolak / Versi bed tidak sesuai") dan mengira sistem sedang rusak, padahal transaksi pertama sebenarnya sudah sukses.

**Rekomendasi.** Tertutupnya modal secara otomatis melalui `FIX-EPS-012-02` akan langsung mengeliminasi risiko ini karena pengguna tidak lagi berinteraksi dengan snapshot data usang.

---

### ISS-EPS-012-T2 — Perilaku identik pada tombol "Batalkan Pesanan"

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source `use-inpatient-bed-board-actions.jsx:108-118` |
| **Perbaikan** | `FIX-EPS-012-01`, `FIX-EPS-012-02` |

**Apa yang terjadi.** Sama seperti alur konfirmasi masuk, ketika petugas memilih untuk membatalkan pesanan tempat tidur (*Cancel Reservation*):
1. Tombol "Batalkan Pesanan" tidak menampilkan state loading saat penyiapan (*prepare*).
2. Setelah pembatalan berhasil dikonfirmasi dan diproses, modal Detail Tempat Tidur tetap terbuka dengan status usang "Dipesan".

**Kenapa terjadi.** Berada pada blok handler dan hook yang sama di `useInpatientBedBoardActions.jsx`.

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG`.

**Dampak nyata.** Alur pembatalan reservasi tempat tidur meninggalkan sisa modal yang tidak konsisten di layar.

**Rekomendasi.** Solusi pada `FIX-EPS-012-01` dan `FIX-EPS-012-02` wajib mencakup penanganan aksi konfirmasi masuk dan pembatalan pesanan secara simetris.

---

## 5. Tanya-jawab pelapor

| Pertanyaan pelapor | Jawaban berbasis analisis teknis |
| --- | --- |
| *"Belum ada state loading atau sudah keluar"* | **Penyebabnya adalah ketiadaan prop `loading` pada tombol `BaseButton` di modal detail.** Saat tombol diklik, sistem sebenarnya sedang menjalankan proses penyiapan asinkron (*prepare*) dengan membaca data terbaru ke backend. Namun karena tombol hanya berstatus `disabled` tanpa animasi *spinner*, antarmuka tampak pasif/diam seolah tidak ada proses berjalan. Tombol baru memunculkan dialog konfirmasi setelah proses pembacaan selesai. Oleh karena itu, perlu ditambahkan state loading visual agar petugas tahu sistem sedang bekerja. |
| *"Ketika saya klik konfirmasi masuk sudah berhasil modal ya tidak tertutup secara otomatis"* | **Penyebabnya adalah hook aksi hanya menutup dialog konfirmasi kecil (`ConfirmModal`) dan lupa memanggil penutupan modal induk (`selectedBed`).** State `selectedBed` di halaman Bed Management tidak pernah direset menjadi `null` setelah request `POST /placements` berhasil. Akibatnya dialog konfirmasi tertutup dan toast hijau muncul, tetapi modal rincian tempat tidur tetap terbuka di layar dengan data lama. |

---

## 6. Temuan tambahan

Selama penelusuran source code, ditemukan bahwa:
1. Tombol **"Batalkan Pesanan"** (`actions.requestCancelReservation`) memiliki cacat arsitektur yang persis sama dengan "Konfirmasi Masuk", karena berbagi mekanisme eksekusi yang sama di hook `useInpatientBedBoardActions`. Perbaikan harus diterapkan secara menyeluruh untuk kedua aksi tersebut.
2. Tidak adanya pembaruan objek `selectedBed` saat `refresh()` berjalan membuat modal induk yang tertinggal menampilkan data yang menyesatkan petugas medis (*stale state*).

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk Pelapor / Pengguna
- Tidak ada pertanyaan tambahan; gejala yang dilaporkan sangat jelas dan telah diverifikasi 100% pada source code frontend.

### Untuk Pemilik Sistem
- **K-01**: Konfirmasi bahwa penutupan otomatis modal Detail Tempat Tidur setelah konfirmasi masuk atau pembatalan pesanan sukses adalah perilaku baku yang disetujui, dan pengguna langsung melihat kartu bed yang diperbarui di papan monitoring. *(Rekomendasi: Setujui)*.

---

## 8. Catatan pola

Pola masalah ini merupakan **Modal Stacking & Callback Lifecycle Mismatch**:
- Ketika sebuah modal induk (Modal A: Detail Bed) memunculkan modal anak/penegas (Modal B: ConfirmModal), penutupan sukses pada Modal B sering kali lupa memicu penutupan pada Modal A karena pembuat hook hanya mengurusi penutupan state lokal miliknya sendiri (`confirmTarget`).
- **Pencegahan ke depan:** Setiap kali hook aksi (`use*Actions`) dipanggil oleh komponen yang memiliki modal pratinjau/detail, hook tersebut wajib menerima kontrak opsi `onSuccess` atau `onCompleted` agar komponen induk dapat membersihkan state tampilannya secara tuntas.

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-13 | AI Engineering Assistant (Antigravity) | Penyusunan awal dokumen analisis issue ISSUE-EPS-012 berdasarkan laporan dan tangkapan layar antarmuka Bed Management. |
| 2026-10-13 | Pemilik Sistem (User) & AI Assistant | Persetujuan rencana perbaikan K-01 dan penyelesaian implementasi perbaikan frontend FIX-EPS-012-01 dan FIX-EPS-012-02. Status issue ditutup SELESAI. |
