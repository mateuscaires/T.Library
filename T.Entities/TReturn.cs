namespace T.Entities
{
  public class TReturn
  {
    public TReturn()
    {
      this.Code = 0;
      this.Success = true;
    }

    public int Code { get; set; }

    public string Message { get; set; }

    public virtual bool Success { get; set; }
  }
}
